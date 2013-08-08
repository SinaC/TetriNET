TODO:
replace assert and return null/false/-1 with error management
player disconnection: quit or connection lost
same tetriminos for every player
[DONE]spam prevention, no more than 1 service call every 50ms (configurable)
[DONE]check if playerId and callback matches when receiving a 'msg' from client
[DONE]server state: 
	waiting start server -> starting server -> waiting start game -> starting game -> game started -> game finished -> waiting start game
	stopping server -> waiting start server
[DONE]client state: 
	application started -> connecting to server -> connected to server -> registering -> waiting start game | application started if registration failed
	waiting start game -> game started -> game finished -> waiting start game

discover:
http://msdn.microsoft.com/en-us/magazine/ee335779.aspx
http://www.freddes.se/2010/05/16/announcement-and-discovery-in-wcf-4/

callback hanging:
http://stackoverflow.com/questions/14393386/prevent-deadlock-issue-with-wcf-duplex-callback-service
http://tech.pro/tutorial/914/wcf-callbacks-hanging-wpf-applications

close gracefully proxy
http://nimtug.org/blogs/damien-mcgivern/archive/2009/05/26/wcf-communicationobjectfaultedexception-quot-cannot-be-used-for-communication-because-it-is-in-the-faulted-state-quot-messagesecurityexception-quot-an-error-occurred-when-verifying-security-for-the-message-quot.aspx
http://stackoverflow.com/questions/530731/how-to-make-sure-you-dont-get-wcf-faulted-state-exception