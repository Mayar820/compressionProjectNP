# Compression Project

A client-server application that compresses files over a TCP connection, built with C# as part of a Network Programming course.

## Overview

The system consists of two parts:

- **Server** — a multi-threaded console app that receives files, compresses them using GZip, and sends them back
- **Client** — a Windows Forms app that lets the user pick a file, send it to the server, and save the compressed result

## How to Run

### Requirements
- Visual Studio 2019 or later
- .NET Framework 4.7+

### Steps
1. Clone or download the solution
2. Open `compressionprojectnp.sln` in Visual Studio
3. Right-click the **Solution** → **Properties** → **Multiple Startup Projects**
4. Set both `CompressionServer` and `CompressionClient` to **Start**
5. Press **F5**

The server console will start first. Then use the client form to connect, browse, and send a file.

## How It Works

```
Client                            Server
  |                                 |
  |-- connect (TCP port 5090) ----> |
  |-- 8 bytes (file size) --------> |
  |-- file bytes -----------------> |
  |                                 | compresses with GZip
  |<-- 8 bytes (compressed size) ---|
  |<-- compressed bytes ------------|
  |                                 |
  | saves file as .gz               | closes connection
```

- File size is sent as a `long` (8 bytes) before the actual data so the receiver knows exactly how many bytes to expect
- The server spawns a new **thread per client**, so multiple clients can connect simultaneously
- `ReceiveExactly` loops until all expected bytes arrive, since TCP does not guarantee data arrives in one chunk

## Code Structure

```
CompressionServer/
└── Program.cs
    ├── Main()           — binds to port 5090, accepts clients in a loop
    ├── HandleClient()   — receives file, compresses it, sends it back (runs on its own thread)
    ├── ReceiveExactly() — helper to receive an exact number of bytes
    └── Compress()       — compresses bytes using GZipStream

CompressionClient/
└── Form1.cs
    ├── button1_Click()  — connects to server
    ├── button2_Click()  — opens file picker and loads file into memory
    ├── button3_Click()  — sends file size + file bytes, receives compressed file and saves it as .gz
    └── ReceiveExactly() — helper to receive an exact number of bytes
```
