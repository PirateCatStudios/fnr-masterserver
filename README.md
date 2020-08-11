### Forge Networking Remastered Master Server

This repository a straight copy of what is on the official [FNR Repository](https://github.com/BeardedManStudios/ForgeNetworkingRemastered).

The code in here was separated out to only include the bare minimum to build the Master Server code.

## How to build

1. Clone the repository
2. Open the `MasterServer.sln` file in Visual Studio
3. Install any missing Nuget packages.
4. Build > Build Solution

### Command line options

-p, --port The port the server will listen on (Default: 15940)
-h, --host The ip address the server will listen on (Default: 0.0.0.0)
-e, --elo The ELO range to use
-d, --daemon Run the the server without user interaction