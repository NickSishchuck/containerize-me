# EventLogger - Docker Tutorial for Complete Beginners

A guide to learning Docker by containerizing a real .NET application.

---

## 📦 What is Docker?

### The Problem Docker Solves

Have you ever heard a developer say "but it works on my machine"? This happens because your development environment is different from your colleague's machine, which is different from the production server. Different operating systems, different installed libraries, different versions of software - it's a nightmare to manage.

Docker solves this by letting you package your application together with everything it needs to run: the operating system, libraries, dependencies, and configuration. This package is called a **container**. When you share a container, you're guaranteeing that it will run exactly the same way on any machine that has Docker installed.

### Containers vs Virtual Machines

You might be thinking "isn't that what virtual machines do?" Yes, but containers are much more lightweight. A virtual machine includes an entire operating system (which can be gigabytes), while a container shares the host operating system's kernel and only packages what your app specifically needs. This means:

- Containers start in seconds (VMs take minutes)
- Containers use megabytes of disk space (VMs use gigabytes)
- You can run dozens of containers easily (VMs are resource-heavy)

### Why Developers Love Docker

Docker has become the industry standard because it makes deployment predictable and reproducible. You build your container once, test it, and then deploy that exact same container to production. No surprises, no "it worked in testing" issues. Plus, Docker makes it trivial to run complex applications with multiple services - databases, caching servers, message queues - all running together in isolated containers.

---

## 🛠️ Prerequisites

Before we start, make sure you have these installed:

1. **Docker Desktop**
   - Download: [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
   - Verify installation: Open terminal and run `docker --version`

2. **.NET 8 SDK**
   - Download: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
   - Verify installation: Run `dotnet --version`

3. **A Text Editor**
   - VS Code, Visual Studio, or any editor you like
   - This project is already set up, you just need to run it

---

## 🔑 Core Concepts (Keywords Explained)

Before we dive in, let's understand the key Docker terminology:

### Image

Think of an image as a **blueprint** or **recipe**. It contains all the instructions and files needed to create a container. Images are immutable (they don't change) and can be shared with others. When you run `docker build`, you create an image.

**Analogy**: Like a cookie cutter - it defines the shape, but isn't the cookie itself.

### Container

A container is a **running instance** of an image. It's your application actually executing, isolated from other containers and the host system. You can create many containers from the same image. When you run `docker run`, you create and start a container.

**Analogy**: The actual cookie made from the cookie cutter. You can make many cookies from one cutter.

### Dockerfile

A text file that contains the **recipe** for building an image. It's a series of instructions telling Docker how to assemble your image: what base operating system to use, what files to copy, what commands to run, etc.

**Analogy**: A recipe card with step-by-step instructions.

### Volume

A **bridge** between your computer's filesystem and the container's filesystem. Containers are isolated, so by default they can't access your files. Volumes let you share folders between your machine and the container.

**Analogy**: A shared Dropbox folder - both you and the container can read/write files to it.

### Multi-stage Build

A technique where you use multiple `FROM` statements in your Dockerfile. Typically you have a "build" stage with all the tools needed to compile your app, and a "runtime" stage with only what's needed to run it. This keeps your final image small.

**Analogy**: You need a full kitchen to bake a cake, but you only need a plate to serve it.

---

## 🚀 Part 1: Building Your First Container (Follow Along)

Let's containerize the EventLogger application step by step!

### Step 1: Understanding the Dockerfile

Open the `Dockerfile` in your project. Let's break down what each line does:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```

**What's happening?** This starts our build stage. We're using Microsoft's official .NET SDK image as our starting point. The `AS build` gives this stage a name we can reference later.

💡 **Why SDK?** The SDK includes everything needed to compile .NET applications (compilers, build tools, etc.).

```dockerfile
WORKDIR /src
```

**What's happening?** Sets the working directory inside the container to `/src`. All subsequent commands will run from this folder.

```dockerfile
COPY EventLogger.csproj .
RUN dotnet restore
```

**What's happening?** Copy only the project file first, then restore dependencies. This is a optimization trick - Docker caches each step, so if your project file doesn't change, Docker won't re-download packages.

💡 **Why separate from the rest?** NuGet packages change less frequently than your code, so this saves time on rebuilds.

```dockerfile
COPY . .
RUN dotnet publish -c Release -o /app/publish
```

**What's happening?** Now copy all your source code and compile it. The `-c Release` flag builds an optimized version. The `-o /app/publish` puts the compiled files in `/app/publish`.

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
```

**What's happening?** We start a NEW stage with just the runtime (no SDK, no build tools). This is the multi-stage build technique.

💡 **Size matters**: The SDK image is ~700MB, but the runtime is only ~200MB. We don't need compilers in production!

```dockerfile
WORKDIR /app
COPY --from=build /app/publish .
```

**What's happening?** Set working directory to `/app` and copy the compiled files from the build stage. Notice `--from=build` - this reaches back to our earlier stage.

```dockerfile
RUN mkdir -p logs
```

**What's happening?** Create a logs directory inside the container where NLog will write files.

```dockerfile
ENV EventProcessor__Iterations=20
ENV EventProcessor__DelayMs=1000
```

**What's happening?** Set default environment variables. The double underscore `__` is .NET's way of representing nested configuration (it maps to `EventProcessor:Iterations` in appsettings.json).

```dockerfile
ENTRYPOINT ["dotnet", "EventLogger.dll"]
```

**What's happening?** This is the command that runs when the container starts. It executes our compiled application.

---

### Step 2: Building the Image

Now let's turn that Dockerfile into an actual image. Open your terminal in the project directory and run:

```bash
docker build -t eventlogger:dotnet .
```

**Breaking down the command:**

- `docker build` - The command to build an image
- `-t eventlogger:dotnet` - Tag (name) our image. Format is `name:tag`. We're calling it "eventlogger" with tag "dotnet"
- `.` - The build context (current directory). Docker will send all files here to the build process

**What you'll see:**

```
[+] Building 45.2s
 => [build 1/5] FROM mcr.microsoft.com/dotnet/sdk:8.0
 => [build 2/5] COPY EventLogger.csproj .
 => [build 3/5] RUN dotnet restore
 => [build 4/5] COPY . .
 => [build 5/5] RUN dotnet publish -c Release -o /app/publish
 => [runtime 1/3] FROM mcr.microsoft.com/dotnet/runtime:8.0
 => [runtime 2/3] COPY --from=build /app/publish .
 => [runtime 3/3] RUN mkdir -p logs
 => => exporting to image
```

💡 **Each line is a layer**: Docker builds images in layers. Each layer is cached, so rebuilds are fast if nothing changed!

**Verify your image was created:**

```bash
docker images
```

You should see `eventlogger` with the `dotnet` tag listed.

---

### Step 3: Running the Container

Time to see your containerized app in action

```bash
docker run --rm -v ./logs:/app/logs eventlogger:dotnet
```

**Breaking down the command:**

- `docker run` - Creates and starts a new container
- `--rm` - Automatically remove the container when it stops (keeps things clean)
- `-v ./logs:/app/logs` - Volume mount! Maps `./logs` on your machine to `/app/logs` in the container
  - Left side (`./logs`) = your computer
  - Right side (`/app/logs`) = inside container
- `eventlogger:dotnet` - The image to run

**What you'll see:**

```
2024-10-03 14:23:15.1234|INFO|EventLogger.Program|=== EventLogger Application Started ===
2024-10-03 14:23:15.1250|INFO|EventLogger.Program|Environment: Production
2024-10-03 14:23:15.1267|INFO|EventLogger.Services.EventProcessor|Event Processor started. Iterations: 20, Delay: 1000ms
2024-10-03 14:23:15.1285|INFO|EventLogger.Services.EventProcessor|[1/20] UserLogin: User user42 logged in successfully
...
```

💡 **Check your logs folder!** While the container runs, look at the `./logs` folder on your machine. You'll see files appearing in real-time! That's the volume mount at work - the container is writing files, and they're showing up on your computer.

**Log files you'll find:**

- `eventlogger-2024-10-03.log` - All events
- `errors-2024-10-03.log` - Errors only
- `eventlogger-2024-10-03.json` - Structured JSON logs
- `internal-nlog.txt` - NLog diagnostics

---

### Step 4: Customizing with Environment Variables

The default configuration runs 20 iterations with 1 second between each. Let's customize this!

#### Example 1: Quick Test (5 iterations, faster)

```bash
docker run --rm \
  -v ./logs:/app/logs \
  -e EventProcessor__Iterations=5 \
  -e EventProcessor__DelayMs=500 \
  eventlogger:dotnet
```

**Breaking down the new flags:**

- `-e EventProcessor__Iterations=5` - Override environment variable (5 iterations instead of 20)
- `-e EventProcessor__DelayMs=500` - Wait 500ms instead of 1000ms between events

💡 **The `\` character**: On Linux/Mac, backslash lets you write a command across multiple lines for readability. On Windows PowerShell, use backtick `` ` `` instead.

#### Example 2: Long Running Process

```bash
docker run --rm \
  -v ./logs:/app/logs \
  -e EventProcessor__Iterations=100 \
  -e EventProcessor__DelayMs=2000 \
  eventlogger:dotnet
```

This will generate 100 events with 2 seconds between each. Great for testing log rotation!

#### Example 3: Rapid Fire

```bash
docker run --rm \
  -v ./logs:/app/logs \
  -e EventProcessor__Iterations=50 \
  -e EventProcessor__DelayMs=100 \
  eventlogger:dotnet
```

50 events with only 100ms delay - watch those logs fill up quickly!

### Available Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `EventProcessor__Iterations` | `20` | Number of events to generate |
| `EventProcessor__DelayMs` | `1000` | Milliseconds to wait between events |

💡 **Remember**: The double underscore `__` in environment variables represents a colon `:` in the JSON configuration. So `EventProcessor__Iterations` overrides the `EventProcessor:Iterations` setting in appsettings.json.

---

## 🎓 Part 2: Advanced - Custom Base Images (Reference)

The Microsoft .NET images are convenient, but you can build containers from scratch using any Linux distribution. This gives you more control over the environment.

### Why Use Custom Base Images?

- **Size optimization**: Alpine Linux images are tiny (~5MB vs ~200MB)
- **Specific requirements**: Some apps need particular Linux distributions
- **Learning**: Understanding what Microsoft's images do for you
- **Flexibility**: Full control over what's installed

### Alpine Linux Variation

Alpine is extremely lightweight and popular for Docker images. Here's how to use it:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EventLogger.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Alpine runtime stage
FROM alpine:latest AS runtime

# Install .NET runtime using Alpine's package manager (apk)
RUN apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    dotnet8-runtime

WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p logs

ENV EventProcessor__Iterations=20
ENV EventProcessor__DelayMs=1000

ENTRYPOINT ["dotnet", "EventLogger.dll"]
```

**Key differences:**

- `FROM alpine:latest` - Start with bare Alpine Linux
- `apk add` - Alpine's package manager (like apt-get for Ubuntu)
- `--no-cache` - Don't store package index locally (saves space)
- Manual dependency installation - You must specify all libraries .NET needs

**Build and run:**

```bash
docker build -t eventlogger:alpine .
docker run --rm -v ./logs:/app/logs eventlogger:alpine
```

**Result**: Image size ~90MB (vs ~210MB for Microsoft's image)

### Arch Linux Variation

Arch Linux uses a rolling release model and pacman package manager:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EventLogger.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Arch runtime stage
FROM archlinux:latest AS runtime

# Install .NET runtime using pacman
RUN pacman -Sy --noconfirm \
    dotnet-runtime \
    && pacman -Scc --noconfirm

WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p logs

ENV EventProcessor__Iterations=20
ENV EventProcessor__DelayMs=1000

ENTRYPOINT ["dotnet", "EventLogger.dll"]
```

**Key differences:**

- `FROM archlinux:latest` - Arch base image
- `pacman -Sy` - Sync and update package database
- `--noconfirm` - Don't prompt for confirmation
- `pacman -Scc` - Clean package cache

**Build and run:**

```bash
docker build -t eventlogger:arch .
docker run --rm -v ./logs:/app/logs eventlogger:arch
```

**Result**: Image size ~400MB (larger due to Arch's full-featured base)

### Comparison Table

| Base Image | Package Manager | Base Size | Final Size | Best For |
|------------|----------------|-----------|------------|----------|
| Microsoft .NET | N/A (pre-built) | ~200MB | ~210MB | Convenience, official support |
| Alpine Linux | apk | ~5MB | ~90MB | Size optimization, production |
| Arch Linux | pacman | ~400MB | ~400MB | Latest packages, development |

💡 **When to use each:**

- **Microsoft images**: Default choice, easiest, officially supported
- **Alpine**: Production deployments where size matters, cost optimization
- **Arch**: Development/testing with bleeding-edge packages, not production

---

## 🧹 Cleanup Guide

Docker images take up disk space. Here's how to clean up when you're done:

### Remove Specific EventLogger Images

```bash
docker rmi eventlogger:dotnet
docker rmi eventlogger:alpine
docker rmi eventlogger:arch
```

**Or remove all at once:**

```bash
docker rmi eventlogger:dotnet eventlogger:alpine eventlogger:arch
```

### Remove All Unused Images

```bash
docker image prune -a
```

⚠️ **Warning**: This removes ALL images not currently used by a container, not just EventLogger!

### Clean Build Cache

Docker caches build layers to speed up rebuilds. To clear this cache:

```bash
docker builder prune -f
```

### See What You Have

**List all images:**

```bash
docker images
```

**List all containers (running and stopped):**

```bash
docker ps -a
```

**See disk usage:**

```bash
docker system df
```

### Nuclear Option (Clean Everything)

⚠️ **DANGER**: This removes all Docker data on your system!

```bash
# Remove all containers
docker rm -f $(docker ps -aq)

# Remove all images
docker rmi -f $(docker images -q)

# Remove all volumes
docker volume prune -f

# Remove all build cache
docker builder prune -a -f
```

---

## 📚 Quick Reference

### Essential Commands

```bash
# Build an image
docker build -t name:tag .

# Run a container
docker run --rm -v ./logs:/app/logs name:tag

# Run with environment variables
docker run --rm -v ./logs:/app/logs -e VAR=value name:tag

# List images
docker images

# List running containers
docker ps

# List all containers
docker ps -a

# Stop a container
docker stop <container-id>

# Remove an image
docker rmi name:tag

# View container logs
docker logs <container-id>

# Access container shell
docker exec -it <container-id> /bin/bash
```

### Project-Specific Commands

```bash
# Build the app locally (without Docker)
dotnet restore
dotnet build
dotnet run

# Publish for containerization
dotnet publish -c Release -o ./publish

# Build Docker image
docker build -t eventlogger:dotnet .

# Run with defaults
docker run --rm -v ./logs:/app/logs eventlogger:dotnet

# Run with custom configuration
docker run --rm \
  -v ./logs:/app/logs \
  -e EventProcessor__Iterations=10 \
  -e EventProcessor__DelayMs=500 \
  eventlogger:dotnet
```

---

## 🔗 Learn More

- **Docker Documentation**: [https://docs.docker.com](https://docs.docker.com)
- **.NET in Docker**: [https://docs.microsoft.com/dotnet/core/docker/](https://docs.microsoft.com/dotnet/core/docker/)
- **Dockerfile Reference**: [https://docs.docker.com/engine/reference/builder/](https://docs.docker.com/engine/reference/builder/)

---

## 🎯 What You've Learned

Congratulations! You now understand:

✅ What Docker is and why it's useful
✅ Core Docker concepts (images, containers, volumes)
✅ How to write a Dockerfile with multi-stage builds
✅ Building and running containers
✅ Passing configuration via environment variables
✅ Volume mounting to persist data
✅ Using different base images
✅ Cleaning up Docker resources

You're ready to containerize your own applications ;)
