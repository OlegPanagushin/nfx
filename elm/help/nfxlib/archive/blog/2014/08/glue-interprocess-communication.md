# NFX.Glue - Interprocess Communication

## Definition + Features
NFX.Glue - is a part of NFX framework that allows developers to quickly (much faster than using WCF/RMI or remoting) interconnect/"glue together" various process instances. 
In one sentence: **NFX.Glue is a contract-based state-less or state-full RPC mechanism that uses messages as logical delivery unit.** 
The core implementation of Glue is probably less than 10,000 LOC (very usual for NFX), and adds roughly 300Kb to the final assembly image.

### NFX.Glue Features
- **Very Simple** - to use and configure
- Built-in NFX application container, so can be **Hosted in any app type without special "service hosts"**
- Contract-based programming
- Injectable binding types define protocol/message exchange patterns (i.e. sync blocking/async/multicast etc)
- Pre-implemented native bindings: TCP sync, TCP async, In-process
- Native bindings allow for **transparent serialization, no need for special attributes** (unlike WCF or ProtoBuf), supports objects of any complexity with cyclical references
- Message-based. Every call turns into RequestMsg, server generates ResponseMsg for two-way calls
- Aupports MessageHeaders for extra data (i.e. security credentials)
- Supports one-way or two-way calls
- Supports multilevel message filtering/inspection (glue/client/binding)
- Supports security - guard contracts/methods/classes with permission attributes
- Supports **state-less or state-full** server programming with volatile process lifecycle (allows process to restart without "forgeting" its state)
- Proxy Clients natively provide sync and async call trampolines without any extra threads or wait queues/reactors
- Built-in **channel/transport lifecycle management** - impose limits on the number of outgoing connections per host etc., how long to keep idle channels alive etc.
- **Detailed statistics** - number of messages/bytes/calls, call **round-trip times per contract/method**
- **Performance** on a 6 core machine: ~120,000 ops/sec two-way simple calls (return int as string+'hello!') via native TCP sync binding

## How NFX.Glue Works
A call is originated from a calling party, like so:
```cs
var node = new Node("async://quad:7311"); 
var console = new RemoteTerminalClient( node );
console.Connect("Jack Lowery");
 
Console.WriteLine("The time on connected node is: " + console.Execute("time");
 
console.Disconnect();
```
Here, we have connected to machine "quad" using "async" for binding. The calling process has a piece of config that says:
```
glue
{
  bindings
  {
    binding {name="async" type="NFX.Glue.Native.MpxBinding, NFX"}
  }
}
```
So now, the Glue runtime knows that "async" is an instance of "NFX.Glue.Native.MpxBinding, NFX" (with about dozen of parameters like TCP buffer windows etc). 
The original contract for the service is this:
```cs
/// <summary>
/// Represents a contract for working with remote entities using terminal/command approach
/// </summary>
[Glued]
[AuthenticationSupport]
[RemoteTerminalOperatorPermission]
[LifeCycle(ServerInstanceMode.Stateful, SysConsts.REMOTE_TERMINAL_TIMEOUT_MS)]
public interface IRemoteTerminal
{
  [Constructor]
  RemoteTerminalInfo Connect(string who);
  
  string Execute(string command);
  
  [Destructor]
  string Disconnect();
}
```
It is a state-full contract that initializes server instance (a terminal connection, in our case) with a call to "Connect" and then either times-out 
after "REMOTE_TERMINAL_TIMEOUT_MS" or gets torn down by a call to "Destructor". In this semantic, constructor/destructor is just a special kind of method 
that does regular method work, possibly returning some parameters but also telling Glue what to do with the instance. 
The "LifeCycle" is a part of the contract not the implementation, because it really dictates what other methods a contract should have/not have. 
Pay attention to "RemoteTerminalOperatorPermission" which guards ALL methods of this contract. 
A user must supply a valid token, for this "AuthenticationSupport" is stipulated.

On the server we will include in config:
```
glue
{
  servers
  {
    server
    {
      name="TerminalAsync"
      node="async://*:7700"
      contract-servers="ahgov.HGovRemoteTerminal, ahgov"
    }
  }
}
```
And then implement the interface like so:
```cs
/// <summary>
/// Provides basic app-management capabilities
/// </summary>
[Serializable]
public class AppRemoteTerminal : IRemoteTerminal
{
    public AppRemoteTerminal()
    {                                                            
      .....
    }
    
    protected override void Destructor()
    {
      .....
    }
 
    private int m_ID;
    private string m_Name;
    private string m_Who;
    private DateTime m_WhenConnected;
    private DateTime m_WhenInteracted;
     
     
    public virtual RemoteTerminalInfo Connect(string who)
    {
      .....
    }
 
    [AppRemoteTerminalPermission]
    public virtual string Execute(string command)
    {
      .....
    }
 
    public virtual string Disconnect()
    {
      return "Good bye!";
    }
 }
```
Notice the use of instance fields.

Lets look at the following diagram:
![Glue-1.png](../../../../images/archive/Glue-1.png)

The call is made in the client code, and then it gets turned into a "RequestMsg". The client transport makes a "CallSlot" - a type of "spirit-less-mailbox"(no threads/events) 
that captures a request with its timestamp and unique GUID. At the end of the call, the server sends ResponseMsg if a call is not OneWay, 
and the response gets matched by the RequestID into the original "CallSlot".

An interesting part of this design is the "Binding" area - it controls the means of message delivery (i.e. TCP/IP/USB/COM/LPT or anything else) 
and the message exchange mode: synchronous or asynchronous. In SYNC mode the message gets sent and response gets delivered in one operation akin to TCP blocking sockets. 
In ASYNC mode we use completion ports on Windows to establish a bi-directional traffic channel per every single socket. 
Those implementations are provided in "NFX.Glue.Native" namespace in "SyncBinding" and "MpxBinding"(MultiplexingBinding). 
"MpxBinding", which is asynchronous by definition, the sending is orthogonal to receiving, what this means is that the physical TCP channel IS NOT BLOCKED 
for the duration of the call execution. For example, suppose the server needs 100msec to execute some method. 
One can post 1000s calls using the same transport via MpxBinding, the responses will arrive as they get generated by the server. 
Had we used "SyncBinding" instead, we would have needed as many TCP connections as currently pending calls, however do not question the need for "SyncBinding". 
Blocking sockets work with much-lower call-roundtrip latency in scenarious when calls are not frequent and not highly-parallel - for example 
local machine clock update done every minute via SyncBinding would work much better time-wise vs. async socket/message IO (+-few milliseconds difference). 
So, "MpxBinding" is better for throughput and tolerable latencies for many calls (1000s/sec), whereas "SyncBinding" is better latency for relatively-seldom calls (10s/sec).

## A Few Q/A
- **How does this relate to ZeroMQ?** - NFX.Glue is a Contract-based/object-level message passing system, whereas ZeroMQ is byte-message oriented. 
NFX.Glue is a much higher-level framework designed to work with higher-level constructs conducive to solving business problems
- **Is Glue slower than ZeroMQ?** - it really depends on what type of "business payload" your app is pushing. 
The network part of Glue is as fast as ZeroMQ as it uses basic sockets and avoids buffer copies whenever possible, 
but please do not compare sending byte[4] with calling a method on a remote class instance
- **How does Glue relate to Erlang?** - a similar answer to the ZeroMQ question above, Erlang works with much lower-level(than Glue) data primitives - tuples, 
lists and the like. One can not really compare the two technologies directly as building the similar feature set in Erlang would require a significant effort 
(add security, permissions, state management), and Erlang uses its own communication platform (OTP) very well, 
however it is still much narrower in scope than NFX.Glue. Take a look at NFX.Erlang instead if you need to support Erlang/OTP from NFX.
- **Does Glue replace completely WCF?** - for us YES, 200%. The whole Aum Cluster is based on Glue, because all nodes in cluster are running NFX, 
it is a benefit of UNISTACK concept that I described a few weeks back. If you are a corporate SOAP/WSE-consumer then NO, 
glue does not support it currently with native bindings and never will. One can create bindings for SOAP and other corporate bloat 
but there is really no need to pollute a clean NFX library with out-dated crap.
- **How do I expose a Glue contract as JSON/REST** - you'd need to use JSONHttp binding for that, the one that I have not created and have no intention to create, 
because it has no practical value. In NFX, REST services are done much easier with NFX.Wave MVC controllers, that should expose your internal Glue services as a facade. 
Remember - Glue was never meant to be exposed publicly, although it could via corresponding bindings, 
but there is no need to create bindings just to support some standards that will never be used.

---
Dmitriy Khmaladze  
August 2, 2014