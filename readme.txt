TODO:
replace assert and return null/false/-1 with error management
replace Tetriminos enum with index
GameStart send players, tetrimino definitions and game rules
spam prevention, no more than 1 service call every 50ms (configurable) -> modify ip filter + handle spam in IPFilterServiceBehavior
use interface instead of RemoteEndpointMessageProperty (server-side)
implement built-in client: new ITetriNETProxyManager and new ITetriNETCallbackManager
remove every reference to network from GameClient and GameServer

discover
http://msdn.microsoft.com/en-us/magazine/ee335779.aspx
http://www.freddes.se/2010/05/16/announcement-and-discovery-in-wcf-4/

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

server->client exception
http://beyondrelational.com/modules/2/blogs/79/posts/11543/throwing-exceptions-from-wcf-service-faultexception.aspx
http://stackoverflow.com/questions/1369882/wcf-web-service-custom-exception-error-to-client
https://go4answers.webhost4life.com/Example/delect-unexpected-dead-client-wcf-67814.aspx

tetris GUI WPF
http://sekagra.com/wp/2011/11/wpf-tetris/