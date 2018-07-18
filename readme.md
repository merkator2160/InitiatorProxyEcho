# Proxy, Initiator and Echo
## Servers
### Initiator Server
The Initiator Server should be provided as a console application. The Initiator Server
gets Proxy Server's IP/Port, sender and receiver threads count (max 3 for each one)
as a command line arguments. It initiates numbers starting from 0 and writes these
numbers in the initiator_send.txt (one number per line) when the command start is
entered from the console. After, the Initiator Server sends these numbers to the
Proxy server. It also receives numbers from the Proxy Server and writes these
numbers in the initiator_receive.txt (one number per line) (pic. 1). It must suspend
when the stop command is entered and must shutdown when the exit command is
entered.
### Echo Server
The Echo Server should be provided as a console application. The Echo Server gets
Proxy Server's IP/Port , sender and receiver threads count (max 3 for each one) as a
command line arguments. It begins to receive the numbers from the Proxy and
writes these numbers in the echo_send.txt (one number per line) when
command start is entered from the console. After, the Echo Server sends these
numbers to the Proxy server. It must shutdown when the exit command is entered.
### Proxy Server
The Proxy Server should be provided as a console application and must provide
communication between the Initiator Server and the Echo Server as mentioned
above. Also it must have start and exit commands.

### Test scenario
We have only one use case:
1. start Proxy Server
2. start Echo Server
3. start Initiator Server
After ~ 10 minutes
4. stop Initiator Server. The Proxy Server must show that the Initiator Server is
suspended (For example, it may print the message in the console that the Initiation
Server has stopped!).
5. start Initiator Server (After the Initiation Server has stopped! Message). The Proxy
Server must show that the Initiator Server is started.
After ~ 10 minutes
6. exit Initiator Server . Proxy Server must show that the Initiator Server has exited
(For example, it may print the message in the console that the Initiation Server has
exited.). Initiator Server exists after receiving all the numbers from the Proxy Server.
After that it must print the message in the console about the successful finish.
7. exit Proxy Server and exit Echo Server
At the end of the test:
Numbers should be written in the sorted form in all the files.
P.S. We expect to see efficient but readable code with clean abstractions.