# textpop_server
SQL Server Installation
1. Open port 80 & 443 for incoming HTTP request in firewall
2. Install IIS and **Websocket from Server Manager** (https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-3.1#iisiis-express-support), it needed for signalR
3. Downlaod and install SQL Server
4. Download the .zip file in github and open in visual studio
5. Install .Net Core Hosting Bundle .Net 6.0 (remember to download correct version) https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/hosting-bundle?view=aspnetcore-8.0
6. Open SQL Server Object Explore in Visual Studio, add SQLEXPRESS server and choose True for Trust Server Certificate
7. Get the connection string then revise the ConnectionStrings in appsettings.json
8. Remove Migration Folder and type command in Package Manage Console (Add-Migration InitialCreate -> Update-Database) https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=vs
9. Install nClam to the Window Server (https://github.com/tekmaven/nClam) (be awared that it used 2GB ram, not used as default in chat image and account update)
10. Copy the wwwroot folder (contains profile icon of account) before publish
11. Open SSMS -> Logins -> New Logins -> Search -> type (LOCAL SERVICE) -> Check Names -> OK
12. Open SSMS -> todolist (database) -> Security -> Users -> New Users -> Login name ... -> User name can be same as Login name -> OK
13. todolist (database) -> Security -> Users -> (NT AUTHORITY\LOCAL SERVICE) -> properties -> membership -> tick db_datareader & db_datawriter & db_owner
14. Publish asp.net core (portable) version in designated folder for IIS
17. Open properties of the designated folder -> Security -> Edit -> type (LOCAL SERVICE) -> add permission for the folder
18. Create a new site in IIS and Change identity of the todolist site to LOCALSERVICE
19. Set up the eturnal turn server for webrtc relay (open firewall and network redirection port for turn and relay, download the .exe file, set the eturnal.yml configuration including secret, public ipv4 address, credentials with password starting with letter)


# Switch Development between Production
1. Comment builder.WebHost.UseUrls("http://0.0.0.0:5000").UseKestrel();
2. Remove comment app.UseHttpsRedirection();
3. Remeber to open the clam antivirus scanner in services


# Eturnal server
1. download and install the eturnal server for window
2. follow the instruction in documentation (https://eturnal.net/doc/)
3. open firewall port 3478 for turn (ucp and udp, inbound), 49152-65535 for udp relay (udp, inbound & outbound) and redirct specific ip to local server 
4. set the eturnal.yml (inside etc folder) configuration as follow and update ipv4 address 
```yml
## Shared secret for deriving temporary TURN credentials (default: $RANDOM):
  ## secret: "long-and-cryptic"
  
  ## The server's public IPv4 address (default: autodetected):
  relay_ipv4_addr: "172.167.162.193"
  ## The server's public IPv6 address (optional):
  #relay_ipv6_addr: "2001:db8::4"

  credentials:
    textpop: Aa123456

  listen:
    -
      ip: "0.0.0.0" //for ipv4, :: for ipv6
      port: 3478
      transport: udp
    -
      ip: "0.0.0.0" //for ipv4, :: for ipv6
      port: 3478
      transport: tcp
```
5. troubleshooting  (https://matrix-org.github.io/synapse/latest/turn-howto.html#troubleshooting)
