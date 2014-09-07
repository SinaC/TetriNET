Server
	1 wait room
	multiple game room

-----
client can connect to server
client can disconnect

Wait room (client)
	client can join/create a game room
	client can send private message
	client can send broadcast message (every player in wait room will receive message)
	client can change team

Game room (player or spectator -> entity)
	entity can vote kick another entity from game room
	entity can start/stop/pause/resume a game (only game room master)
	entity can change game room options (only game room master)
	entity can reset winlist (only game room master)
	entity can send private message
	entity can send broadcast message (every entity in game room will receive message)
	entity can change team
	player can place piece
	player can send special to another player
	player can win/lose a game

Ideas: 
	one endpoint for wait room and one endpoint for game room with own contract/callback
	ban handling