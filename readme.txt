TODO:
player disconnection: quit or connection lost
spam prevention, no more than 1 service call every 50ms (configurable)
server state: 
	waiting start game -> starting game -> game started -> game finished -> waiting start game
client state: 
	application started -> connecting to server -> connected to server -> registering -> waiting start game | application started if registration failed
	waiting start game -> game started -> game finished -> waiting start game

discover:
http://msdn.microsoft.com/en-us/magazine/ee335779.aspx

callback hanging:
http://stackoverflow.com/questions/14393386/prevent-deadlock-issue-with-wcf-duplex-callback-service
http://tech.pro/tutorial/914/wcf-callbacks-hanging-wpf-applications