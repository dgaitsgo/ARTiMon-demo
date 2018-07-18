#include <stdio.h>
#include <string.h>
#include <stdlib.h>

#include <arpa/inet.h>
#include <sys/socket.h>

#include <jansson.h>

#include "artimon.h"

# define N_SENSORS 9 
# define BUFF_SIZE 1024

char	*ft_itoa(int n);

enum	e_sensor
{
	GLOBAL_TIME,
	ENEMY_SPOTTED,
	ATHZ_TO_CONTINUE,
	FLEX,
	HEART_RATE,
	BODY_TEMP,
	GLUCOSE,
	HYDRATION,
	OXYGEN,
	FLEXIONS,	
};

typedef struct			s_artimon
{
	int			nb_atoms;
	int			nb_sig;
	int			nb_main;
	int			nb_warn;
	unsigned int 		sigtab[2];
	double			*hazard_detect_table;
	double			*main_reset;
	double			*warn_value_tab;
	notif_42		**notif_tab;
}				t_artimon;

typedef struct			s_socket
{
	int			socket_desc;
	int			client_socket_desc;
	int			bytes_read;
	socklen_t		addr_len;
	struct sockaddr_in	server;
}				t_socket;

typedef struct			s_readtools
{
	char			buffer[BUFF_SIZE];
	char			*token[N_SENSORS];
	int			bytes_read;
}				t_readtools;

// a container to treate sensor array data as first class memory
typedef struct			s_sensor
{
	double			array[N_SENSORS];
}				t_sensor;

char				*path_to_db = "/home/pi/localhost/db/json.db";

void	error_exit(const char *message, int line)
{
	printf("%d : %s\n", line, message);
	exit(1);
}

void	set_socket(t_socket *sock, const char *host)
{
	sock->socket_desc = socket(AF_INET, SOCK_STREAM, 0);
	if (sock->socket_desc == -1)
		error_exit("Could not open socket", __LINE__);
	sock->server.sin_addr.s_addr = inet_addr(host);
	sock->server.sin_family = AF_INET;
	sock->server.sin_port = htons(50001);
	bind(sock->socket_desc, (struct sockaddr *)&sock->server, sizeof(sock->server));
	listen(sock->socket_desc, 1);
	printf("Waiting for env...\n");
	sock->addr_len = sizeof(struct sockaddr_in);
	sock->client_socket_desc =
	accept(sock->socket_desc, (struct sockaddr *)&sock->server, &sock->addr_len);
	if (sock->client_socket_desc < 0)
		error_exit("Could not accept client", __LINE__);
}

void		init_artimon(t_artimon *a)
{
	if (init_artimon_embedded_4_42_v2(&a->nb_atoms,
					&a->nb_sig,
					1,
					&a->nb_main,
					&a->notif_tab))
		{
			error_exit("Could not initialize artimon", __LINE__);
		}
}

void	tokenize(t_readtools *reader)
{
	int i = 0;
	while (i < N_SENSORS)
	{
		if (i == 0)
			reader->token[i] = strtok(reader->buffer, " ");
		else
			reader->token[i] = strtok(NULL, " ");
		i++;
	}
}

void		unity_string_to_sensor_data(t_readtools *reader, t_sensor *sensor)
{
	int i = 0;
	while (i < N_SENSORS)
	{
		sensor->array[i] = (double)atof(reader->token[i]);
		i++;
	}
}

void		sensor_data_to_artimon(t_sensor *sensor, t_artimon *artimon)
{
	//global time not refreshed here
	int i = 1;
	while (i < N_SENSORS)
	{
		artimon->sigtab[0] = (* ((unsigned *) &sensor->array[i]));
		artimon->sigtab[1] = (*(((unsigned *) &sensor->array[i]) + 1));
		artimon_refresh_signal(i - 1, artimon->sigtab);
		i++;
	}
}

json_t	*build_json_db(t_artimon *a)
{
	json_t	*db;
	int	i;

	db = json_object();
	i = 0;
	while (i < a->nb_main)
	{
		json_object_set(db, ft_itoa(i), json_pack("{sisbsbss?ss?}",
					"id", i,
					"active", 0,
					"queued", 0,
					"description", a->notif_tab[i]->description,
					"instruction", a->notif_tab[i]->suggest));
		i++;
	}
	json_dump_file(db, path_to_db, 0);
	return (db);
}

void		check_alerts(t_socket *send, t_artimon *artimon, json_t *db)
{
	int i = 0;
	double		*warning_table = artimon->warn_value_tab;
	notif_42 	**notif_tab = artimon->notif_tab;

	while (i < artimon->nb_main)
	{
		if (notif_tab[i]->stopped)
		{
			if (notif_tab[i]->is_a_warning)
			{
				json_object_set(json_object_get(db, ft_itoa(i)), "active", json_true());
			}
			else
			{
				json_object_set(json_object_get(db, ft_itoa(i)), "active", json_false());
			}
		}
		i++;	
	}
}

void		set_send_socket(t_socket *sock, const char *host)
{
	sock->socket_desc = socket(AF_INET, SOCK_STREAM, 0);
	if (sock->socket_desc == -1)
	{
		error_exit("Could not open socket", __LINE__);
	}
	sock->server.sin_family = AF_INET;
	sock->server.sin_addr.s_addr = INADDR_ANY;
	sock->server.sin_port = htons(50002);
	bind(sock->socket_desc, (struct sockaddr *)&sock->server, sizeof(sock->server));
	listen(sock->socket_desc, 1);
	printf("Waiting for site ... \n");
	sock->addr_len = sizeof(struct sockaddr_in);
	sock->client_socket_desc = 
	accept(sock->socket_desc, (struct sockaddr *)&sock->server, &sock->addr_len);
	if (sock->client_socket_desc < 0)
		error_exit("Could not accept client", __LINE__);
}


int		main(void)
{
	t_socket		listen_unity;
	t_socket		send_site;
	t_artimon		artimon;
	t_sensor		sensor;
	t_readtools		reader;
	json_t			*db;

	set_send_socket(&send_site, "172.20.10.3");
	set_socket(&listen_unity, "172.20.10.3");
	init_artimon(&artimon);
	db = build_json_db(&artimon);
	while (1)
	{
		check_alerts(&send_site, &artimon, db);
		reader.bytes_read = recv(listen_unity.client_socket_desc, reader.buffer, BUFF_SIZE, 0);
		if (reader.bytes_read > 0)
		{
			reader.buffer[reader.bytes_read] = '\0';
			tokenize(&reader);
			unity_string_to_sensor_data(&reader, &sensor);
			sensor_data_to_artimon(&sensor, &artimon);
			char *dump = json_dumps(db, 0);
			write(send_site.client_socket_desc, dump, strlen(dump));
			free(dump);

			artimon_refresh_time(sensor.array[GLOBAL_TIME]);
			printf("\n\nGlobal Time: %f | Enemy spotted : %f | Athz to Cont : %f | Flex : %f | Heart Rate : %f | Body Temp : %f | Glucose : %f | Hydration : %f | Oxygen : %f | Flexions : %f\n",
			sensor.array[GLOBAL_TIME],
			sensor.array[ENEMY_SPOTTED],
			sensor.array[ATHZ_TO_CONTINUE],
			sensor.array[FLEX],
			sensor.array[HEART_RATE],
			sensor.array[BODY_TEMP],
			sensor.array[GLUCOSE],
			sensor.array[HYDRATION],
			sensor.array[OXYGEN],
			sensor.array[FLEXIONS]);
		}
	}
	return (0);
}
