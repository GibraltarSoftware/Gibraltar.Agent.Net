Loupe Agent for Networking
===================

This simple agent adds network monitoring capabilities to any .NET application.  
It extends the [Loupe Agent](http://nuget.org/GibraltarSoft/Gibraltar.Agent) so you can 
use any viewer for Loupe to visualize network information.

Connectivity Monitor
--------------------

This class is designed to perform non-invasive continuous monitoring of an IP address or host name.
It uses Ping requests to measure round drip latency and availability.  It will record an information
message whenever connectivity is lost or regained to a monitored IP address/host name.  Any number of
these can be run in a process and they work entirely in the background.  

Implementation Notes
--------------------

Since Loupe supports .NET 2.0 and later and the monitored networking capabilties are available
in .NET 2.0 this agent targets .NET 2.0 as well.  Due to the built-in compatibility handling in the
.NET runtime it can be used by any subsequent verison of .NET so there's no need for a .NET 4.0 or later
version unless modifying to support something only available in .NET 4.0 or later.


Building the Agent
------------------

This project is designed for use with Visual Studio 2012 with NuGet package restore enabled.
When you build it the first time it will retrieve dependencies from NuGet.

Contributing
------------

Feel free to branch this project and contribute a pull request to the development branch. 
If your changes are incorporated into the master version they'll be published out to NuGet for
everyone to use!