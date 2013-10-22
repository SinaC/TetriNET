TODO:
spam prevention, no more than 1 service call every 50ms (configurable) -> modify ip filter + handle spam in IPFilterServiceBehavior
Manage new connection and disconnection at one place -> ideally in Server because there can be many hosts and we can remove a player only once
when Server calls BanPlayer on hosts, only one host must add it to ban manager:
	use a common object for every transport address, ban list Add and isBanned take this common object instead of a real transport address
	-> everyone can manage ban without knowing in which host it has been created
	-> this object is stored in IPlayer and created by createPlayerFunc in host so server or host can ban a player using IPlayer info
client/server version when registering
SinaCSpecials: use gravity when board has enough holes or when to high + zebra + clear column + confusion + darkness + immunity + left gravity + mutation
WPF Client bot: sometimes _handleNextTetriminoEvent is not raised or is forgot
Confusion could be managed client-side
Add Pentominos (http://en.wikipedia.org/wiki/Pentomino)
New options: random blocks clear count, mutation count, darkness time, confusion time, immunity time, random spawn orientation
Solo mode (score, t-spin)
Server integrated in WPF client

hold: store a piece for later use --> this could lead to problem with server (server checks piece id when receiving PlaceTetrimino msg  line 636)

new client->server API: EarnAchievement(id, title)  (server doesn't know anything about achievements)
new server->client API: OnAchievementEarned(playerId, id, title)
when receiving OnAchievementEarned, display msg in party line chat and in game chat

bugs:
why user.config is automatically created with default values in $APPDATA$/Local/SinaC/TetriNET WPF WCF Client/  when recompiling a new version
tetriminoI appears on row 21 instead of 22
FIXED	sometimes bottom line seems to disappear (occurs when lines are added by server) --> GRRRRRR DUMB DEVELOPER
focus on game view is not set properly when starting a game and player is in stat tab or options tab
chat player list is sometimes wrong (2 times the same player) server not yet started and 2 clients try to connect at the same time
server sometimes doesn't send next pieces to client or is too slow  (400ms elapsed between PlacePiece and OnNextPiece !!!!!)    maybe it's an event problem (the same as bot one)
		Client->Server: PlacePiece(placed piece index, max index in local piece array)
		Server->Client: OnNextPiece(3 next pieces from max index in local piece array from client)
	sample of slow behaviour
		2013-10-18 09:54:19.5323 6 DEBUG PlacePiece 42 TetriminoI 2 5,6 56							<-- PlecePiece(42) received
		2013-10-18 09:54:19.5323 6 DEBUG SendLines 1
		2013-10-18 09:54:19.5479 3 INFO ModifyGrid[CONSOLE_5d6c1]
		2013-10-18 09:54:19.5635 3 INFO PlacePiece[CONSOLE_5d6c1]31:TetriminoL 4 at 12,4 19
		2013-10-18 09:54:19.5635 6 DEBUG PlacePiece 33 TetriminoZ 2 5,11 52
		2013-10-18 09:54:19.5791 3 INFO UseSpecial[CONSOLE_08927][CONSOLE_f6a92]BlockBomb
		2013-10-18 09:54:19.5791 6 DEBUG PlacePiece 34 TetriminoT 3 4,4 53
		2013-10-18 09:54:19.5947 3 INFO PlacePiece[CONSOLE_92e1d]30:TetriminoO 1 at 2,5 41
		2013-10-18 09:54:19.5947 6 DEBUG PlacePiece 33 TetriminoZ 4 10,3 27
		2013-10-18 09:54:19.6103 6 DEBUG ModifyGrid 27
		2013-10-18 09:54:19.6103 3 INFO PlacePiece[CONSOLE_f6a92]30:TetriminoO 1 at 4,4 27
		2013-10-18 09:54:19.6103 6 DEBUG PlacePiece 43 TetriminoI 4 8,5 48							<-- PlecePiece(43) received
		2013-10-18 09:54:19.6103 6 DEBUG PlacePiece 35 TetriminoL 1 9,3 34
		2013-10-18 09:54:19.6259 3 INFO UseSpecial[CONSOLE_f6a92][CONSOLE_92e1d]ClearSpecialBlocks
		2013-10-18 09:54:19.6259 6 DEBUG ModifyGrid 52
		2013-10-18 09:54:19.6415 3 INFO PlacePiece[Joel]39:TetriminoZ 4 at 10,6 68
		2013-10-18 09:54:19.6571 3 INFO PlacePiece[CONSOLE_d489b]30:TetriminoO 1 at 2,8 64
		2013-10-18 09:54:19.6571 6 DEBUG PlacePiece 34 TetriminoZ 1 10,5 56
		2013-10-18 09:54:19.6727 3 INFO ModifyGrid[CONSOLE_5d6c1]
		2013-10-18 09:54:19.6727 6 DEBUG PlacePiece 33 TetriminoI 4 12,8 76
		2013-10-18 09:54:19.6883 3 INFO PlacePiece[CONSOLE_92e1d]31:TetriminoL 3 at 4,4 45
		2013-10-18 09:54:19.6883 6 DEBUG PlacePiece 44 TetriminoO 1 9,6 52							<-- PlecePiece(44) received
		2013-10-18 09:54:19.7039 3 INFO PlacePiece[CONSOLE_5d6c1]32:TetriminoI 2 at 1,6 22
		2013-10-18 09:54:19.7039 6 DEBUG PlacePiece 35 TetriminoL 4 10,5 57
		2013-10-18 09:54:19.7195 3 INFO PlacePiece[CONSOLE_08927]32:TetriminoI 4 at 13,8 45
		2013-10-18 09:54:19.7195 6 DEBUG PlacePiece 36 TetriminoO 1 6,2 26
		2013-10-18 09:54:19.7351 3 INFO PlacePiece[CONSOLE_f6a92]31:TetriminoL 4 at 12,4 31
		2013-10-18 09:54:19.7507 3 INFO PlacePiece[CONSOLE_d489b]31:TetriminoL 3 at 10,5 68
		2013-10-18 09:54:19.7507 6 DEBUG PlacePiece 34 TetriminoZ 4 11,4 31
		2013-10-18 09:54:19.7507 17 DEBUG PlacePiece 35 TetriminoZ 1 9,6 60
		2013-10-18 09:54:19.7663 3 INFO PlacePiece[Joel]40:TetriminoL 2 at 1,6 72
		2013-10-18 09:54:19.7819 3 INFO UseSpecial[CONSOLE_f6a92][CONSOLE_92e1d]ClearSpecialBlocks
		2013-10-18 09:54:19.7819 17 DEBUG PlacePiece 34 TetriminoT 4 10,7 80
		2013-10-18 09:54:19.7975 3 INFO PlacePiece[CONSOLE_5d6c1]33:TetriminoI 2 at 2,6 26
		2013-10-18 09:54:19.7975 17 DEBUG PlacePiece 36 TetriminoO 1 7,4 49
		2013-10-18 09:54:19.8131 3 INFO PlacePiece[Joel]41:TetriminoO 1 at 2,9 76
		2013-10-18 09:54:19.8131 17 DEBUG PlacePiece 45 TetriminoZ 4 11,4 56
		2013-10-18 09:54:19.8287 3 INFO PlacePiece[CONSOLE_92e1d]32:TetriminoI 4 at 13,7 49
		2013-10-18 09:54:19.8287 6 DEBUG PlacePiece 37 TetriminoO 1 4,3 30
		2013-10-18 09:54:19.8443 3 INFO PlacePiece[CONSOLE_08927]33:TetriminoI 4 at 12,8 49
		2013-10-18 09:54:19.8599 3 INFO PlacePiece[CONSOLE_f6a92]32:TetriminoZ 4 at 9,3 23
		2013-10-18 09:54:19.8599 17 DEBUG PlacePiece 35 TetriminoZ 1 2,3 23
		2013-10-18 09:54:19.8755 3 INFO ModifyGrid[CONSOLE_92e1d]
		2013-10-18 09:54:19.8911 3 INFO PlacePiece[CONSOLE_d489b]32:TetriminoI 4 at 13,10 72
		2013-10-18 09:54:19.8911 17 DEBUG ModifyGrid 60
		2013-10-18 09:54:19.8911 17 DEBUG PlacePiece 35 TetriminoL 4 6,7 72
		2013-10-18 09:54:19.9067 3 INFO PlacePiece[CONSOLE_5d6c1]34:TetriminoT 1 at 8,2 30
		2013-10-18 09:54:19.9067 17 DEBUG PlacePiece 37 TetriminoO 1 6,5 53
		2013-10-18 09:54:19.9067 17 DEBUG PlacePiece 46 TetriminoZ 2 5,5 48
		2013-10-18 09:54:19.9223 3 INFO PlacePiece[Joel]42:TetriminoI 2 at 5,6 56				<-- PlacePiece(42) dequeued from game action queue
		2013-10-18 09:54:19.9223 17 DEBUG PlacePiece 36 TetriminoZ 4 11,7 64
		2013-10-18 09:54:19.9379 3 INFO SendLines[Joel]:1
		2013-10-18 09:54:19.9379 6 DEBUG ModifyGrid 34
		2013-10-18 09:54:19.9379 17 DEBUG ModifyGrid 64
		2013-10-18 09:54:19.9379 6 DEBUG ModifyGrid 83
		2013-10-18 09:54:19.9535 3 INFO PlacePiece[CONSOLE_92e1d]33:TetriminoZ 2 at 5,11 52
		2013-10-18 09:54:19.9535 14 DEBUG ModifyGrid 75
		2013-10-18 09:54:19.9691 3 INFO PlacePiece[CONSOLE_08927]34:TetriminoT 3 at 4,4 53
		2013-10-18 09:54:19.9691 14 DEBUG PlacePiece 36 TetriminoZ 1 7,4 38
		2013-10-18 09:54:19.9847 3 INFO PlacePiece[CONSOLE_f6a92]33:TetriminoZ 4 at 10,3 27
		2013-10-18 09:54:19.9847 14 DEBUG PlacePiece 38 TetriminoI 4 13,6 34
		2013-10-18 09:54:20.0003 3 INFO ModifyGrid[CONSOLE_f6a92]
		2013-10-18 09:54:20.0003 14 DEBUG ModifyGrid 45
		2013-10-18 09:54:20.0003 14 DEBUG PlacePiece 36 TetriminoO 1 8,8 87
		2013-10-18 09:54:20.0159 3 INFO PlacePiece[Joel]43:TetriminoI 4 at 8,5 48				<-- PlacePiece(43) dequeued from game action queue
		2013-10-18 09:54:20.0315 3 INFO PlacePiece[CONSOLE_5d6c1]35:TetriminoL 1 at 9,3 34
		2013-10-18 09:54:20.0471 3 INFO ModifyGrid[CONSOLE_92e1d]
		2013-10-18 09:54:20.0627 3 INFO PlacePiece[CONSOLE_92e1d]34:TetriminoZ 1 at 10,5 56
		2013-10-18 09:54:20.0627 6 DEBUG PlacePiece 37 TetriminoZ 1 5,4 42
		2013-10-18 09:54:20.0627 17 DEBUG PlacePiece 37 TetriminoZ 2 3,7 79
		2013-10-18 09:54:20.0627 17 DEBUG PlacePiece 47 TetriminoI 4 13,6 52
		2013-10-18 09:54:20.0783 3 INFO PlacePiece[CONSOLE_d489b]33:TetriminoI 4 at 12,8 76
		2013-10-18 09:54:20.0783 17 DEBUG PlacePiece 38 TetriminoI 2 1,9 68
		2013-10-18 09:54:20.0939 3 INFO PlacePiece[Joel]44:TetriminoO 1 at 9,6 52				<-- PlacePiece(44) dequeued from game action queue

wcf + jquery
http://stackoverflow.com/questions/4336212/return-json-data-from-a-wcf-service-with-nettcpbinding
http://www.codeproject.com/Articles/132809/Calling-WCF-Services-using-jQuery
http://www.codeproject.com/Questions/604639/howplustoplusconsumepluswcfplusservicepluswithplus
http://www.dotnetcurry.com/ShowArticle.aspx?ID=728
http://www.codeproject.com/Articles/223572/Calling-Cross-Domain-WCF-service-using-Jquery-Java
http://pranayamr.blogspot.be/2010/11/create-hostself-hosting-iis-hosting-and.html
http://pranayamr.blogspot.be/2010/12/steps-to-call-wcf-service-using-jquery.html
http://stackoverflow.com/questions/885744/wcf-servicehost-access-rights
http://stackoverflow.com/questions/3684641/wcf-self-hosting-jquery
http://bendewey.wordpress.com/2009/11/24/using-jsonp-with-wcf-and-jquery/
http://stackoverflow.com/questions/11684623/consuming-wcf-service-application-from-jquery-ajax
http://stackoverflow.com/questions/15582284/json-callback-error-was-not-called-wcf-jquery
http://stackoverflow.com/questions/4361756/jquery-success-callback-called-with-empty-response-when-wcf-method-throws-an-exc

wcf
http://stackoverflow.com/questions/8790665/online-multiplayer-game-using-wcf
http://gafferongames.com/networking-for-game-programmers/udp-vs-tcp/
http://msdn.microsoft.com/en-us/library/ms751494.aspx
http://blog.the-blair.com/2010/04/02/wcf-tutorial.html
http://www.codeproject.com/Articles/331599/What-s-new-in-WCF-4-5-UDP-transport-support
http://webman.developpez.com/articles/dotnet/wcf-4-multicasting/

discover
http://msdn.microsoft.com/en-us/magazine/ee335779.aspx
http://www.freddes.se/2010/05/16/announcement-and-discovery-in-wcf-4/
http://social.msdn.microsoft.com/Forums/vstudio/en-US/256b272b-9885-40ae-b512-8253815c2a14/trying-to-get-example-wcf-discovery-to-work-on-multiple-machines
http://stackoverflow.com/questions/14059493/wcf-discovery-doesnt-work-between-two-computers-on-the-same-subnet-with-the-fir

callback hanging
http://stackoverflow.com/questions/14393386/prevent-deadlock-issue-with-wcf-duplex-callback-service
http://tech.pro/tutorial/914/wcf-callbacks-hanging-wpf-applications

close gracefully proxy
http://nimtug.org/blogs/damien-mcgivern/archive/2009/05/26/wcf-communicationobjectfaultedexception-quot-cannot-be-used-for-communication-because-it-is-in-the-faulted-state-quot-messagesecurityexception-quot-an-error-occurred-when-verifying-security-for-the-message-quot.aspx
http://stackoverflow.com/questions/530731/how-to-make-sure-you-dont-get-wcf-faulted-state-exception
http://stackoverflow.com/questions/1400010/closing-wcf-connection

block incoming connection
http://stackoverflow.com/questions/12089879/how-to-block-incoming-connections-from-specific-addresses
http://stackoverflow.com/questions/1922551/wcf-basichttpbinding-stop-new-connections-but-allow-existing-connections-to-co
http://stackoverflow.com/questions/13174246/how-to-restrict-access-to-wcf-restful-function-to-specific-ip-range
http://www.codeproject.com/Articles/37280/WCF-Service-Behavior-Example-IPFilter-Allow-Deny-A
http://stackoverflow.com/questions/12376068/how-can-i-add-an-ipfilter-to-an-wcf-odata-service
http://stackoverflow.com/questions/722008/can-i-setup-an-ip-filter-for-a-wcf-service
http://keyvan.io/detect-client-ip-in-wcf-3-5
http://blogs.msdn.com/b/carlosfigueira/archive/2011/04/19/wcf-extensibility-message-inspectors.aspx

server->client exception
http://beyondrelational.com/modules/2/blogs/79/posts/11543/throwing-exceptions-from-wcf-service-faultexception.aspx
http://stackoverflow.com/questions/1369882/wcf-web-service-custom-exception-error-to-client
https://go4answers.webhost4life.com/Example/delect-unexpected-dead-client-wcf-67814.aspx

wpf image
http://www.codeproject.com/Articles/25672/Simple-slide-game-using-ViewBox
http://stackoverflow.com/questions/1195208/wpf-how-can-i-show-a-cropped-region-of-an-imagesource-in-an-image-control

tetris GUI WPF
http://sekagra.com/wp/2011/11/wpf-tetris/

console application .NET
http://broadcast.oreilly.com/2010/08/understanding-c-text-mode-games.html
http://www.c-sharpcorner.com/uploadfile/scottlysle/colorful-console-mode-applications-in-C-Sharp/

various
https://github.com/clariuslabs/reactivesockets
http://social.msdn.microsoft.com/Forums/en-US/5c62e690-2c8d-4f32-8ec4-5e9b5ea6d2a0/using-reactive-extensions-rx-for-socket-programming-practical
http://www.cachelog.net/using-reactive-extensions-rx-tpl-for-socket-programming/
https://developers.google.com/protocol-buffers/docs/overview
http://knockoutjs.com/
http://stackoverflow.com/questions/17232183/upload-image-from-phonegap-app-to-wcf-service
http://channel9.msdn.com/coding4fun/blog/Using-HTML5-web-sockets-and-some-C-to-build-a-multiplayer-Space-Shooter
http://www.codeproject.com/Articles/209041/HTML5-Web-Socket-in-Essence
http://www.discountasp.net/

signalr
http://www.asp.net/signalr/overview/getting-started/tutorial-signalr-self-host
http://signalr.net/
https://github.com/SignalR/SignalR/wiki
http://www.hanselman.com/blog/AsynchronousScalableWebApplicationsWithRealtimePersistentLongrunningConnectionsWithSignalR.aspx
http://shockbyte.net/2012/05/30/signalr-samples/
http://stackoverflow.com/questions/7872589/call-specific-client-from-signalr
http://www.codeproject.com/Tips/417502/Online-Whiteboard-using-HTML5-and-SignalR
http://www.asp.net/signalr/overview/hubs-api/hubs-api-guide-server
http://www.west-wind.com/weblog/posts/2013/Sep/04/SelfHosting-SignalR-in-a-Windows-Service
http://stackoverflow.com/questions/17676464/hosting-signalr-with-self-host-in-windows-service
http://sachabarbs.wordpress.com/2013/03/19/signalr-self-hosting-with-net-client/
http://www.asp.net/signalr/overview/getting-started/tutorial-signalr-self-host

Tetrinet original description
http://gtetrinet.sourceforge.net/tetrinet.txt

bot
http://luckytoilet.wordpress.com/2011/05/27/coding-a-tetris-ai-using-a-genetic-algorithm/
http://www.ccs.neu.edu/home/punkball/tetris/
http://www.colinfahey.com/tetris/tetris.html
http://www.cs.cornell.edu/boom/1999sp/projects/tetris/
http://tetrisconcept.net/wiki/Tetris_AI
http://www.ryanheise.com/tetris/tetris_artificial_intelligence.html
http://www.vidarholen.net/contents/junk/tetris/
http://hal.archives-ouvertes.fr/docs/00/41/89/54/PDF/article.pdf
http://totologic.blogspot.be/2013/03/tetris-ai-explained.html
http://lgames.sourceforge.net/index.php?project=LTris
http://en.sfml-dev.org/forums/index.php?topic=10719.0
http://meritology.com/library/public/SmartAgent%20-%20Creating%20Reinforcement%20Learning%20Tetris%20AI.pdf
https://code.google.com/p/tetris-challenge/
http://harddrop.com/forums/index.php?showtopic=1138

tetris
http://jetrix.sourceforge.net/dev-guide.php
http://en.wikipedia.org/wiki/TetriNET
http://tetris.wikia.com/wiki/ARS
http://tetris.wikia.com/wiki/Nintendo_Rotation_System
http://tetris.wikia.com/wiki/Original_Rotation_System
http://tetris.wikia.com/wiki/SRS
http://tetris.wikia.com/wiki/DTET_Rotation_System
http://tetris.wikia.com/wiki/TetriNet_Rotation_System
http://tetrisconcept.net/wiki/TetriNet_Rotation_System
http://harddrop.com/wiki/Tetrinet2
http://blockbattle.net/tutorial

custom user settings path
http://stackoverflow.com/questions/2265271/custom-path-of-the-user-config
http://stackoverflow.com/questions/175726/c-create-new-settings-at-run-time/1236190#1236190
http://msdn.microsoft.com/en-us/library/saa62613(v=VS.100).aspx