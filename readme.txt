TODO:
spam prevention, no more than 1 service call every 50ms (configurable) -> modify ip filter + handle spam in IPFilterServiceBehavior
Manage new connection and disconnection at one place -> ideally in Server because there can be many hosts and we can remove a player only once
when Server calls BanPlayer on hosts, only one host must add it to ban manager:
	use a common object for every transport address, ban list Add and isBanned take this common object instead of a real transport address
	-> everyone can manage ban without knowing in which host it has been created
	-> this object is stored in IPlayer and created by createPlayerFunc in host so server or host can ban a player using IPlayer info
discovery doesn't work on cross/multiple machine
client/server version used when registering
add room notion in server (server can handle multiple rooms and client register on a room)
SinaCSpecials: use gravity when board has enough holes or when to high
GameController -> interface
Remove PierreDellacherieOnePieceBot from every project and create a new bot using 2 strategies, complete special strategy, add a method in IClient to get playing opponents,
	transform MoveStrategyBase and SpecialStrategyBase in interface, add every static method from Dellacherie in BoardHelper
log path/filename should be read from App.config

AI: finish SinaCSpecials, use special advices in Bot
WPF client: options, inventory, main window size, connect, player list

wcf
http://stackoverflow.com/questions/8790665/online-multiplayer-game-using-wcf
http://gafferongames.com/networking-for-game-programmers/udp-vs-tcp/
http://msdn.microsoft.com/en-us/library/ms751494.aspx
http://blog.the-blair.com/2010/04/02/wcf-tutorial.html
http://www.codeproject.com/Articles/331599/What-s-new-in-WCF-4-5-UDP-transport-support

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
http://signalr.net/
http://www.hanselman.com/blog/AsynchronousScalableWebApplicationsWithRealtimePersistentLongrunningConnectionsWithSignalR.aspx
https://github.com/clariuslabs/reactivesockets
http://social.msdn.microsoft.com/Forums/en-US/5c62e690-2c8d-4f32-8ec4-5e9b5ea6d2a0/using-reactive-extensions-rx-for-socket-programming-practical
http://www.cachelog.net/using-reactive-extensions-rx-tpl-for-socket-programming/
https://developers.google.com/protocol-buffers/docs/overview

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

tetris
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
