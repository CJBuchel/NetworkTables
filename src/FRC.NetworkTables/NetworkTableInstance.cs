﻿using FRC.NetworkTables.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FRC.NetworkTables
{
    public partial class NetworkTableInstance : IDisposable
    {
        public const int DefaultPort = 1735;
        private bool m_owned;

        private NetworkTableInstance(NT_Inst handle)
        {
            m_owned = false;
            Handle = handle;

            m_entryListenerToken = new Lazy<CancellationTokenSource>(CreateEntryListenerThread);
            m_connectionListenerToken = new Lazy<CancellationTokenSource>(CreateConnectionListenerThread);
            m_rpcListenerToken = new Lazy<CancellationTokenSource>(CreateRpcListenerThread);
            m_loggerListenerToken = new Lazy<CancellationTokenSource>(CreateLoggerThread);
        }

        public void Dispose()
        {
            if (m_owned && Handle.Get() != 0)
            {
                NtCore.DestroyInstance(Handle);
                Handle = new NT_Inst();
            }
        }

        public bool IsValid() => Handle.Get() != 0;

        private static Lazy<NetworkTableInstance> s_defaultInstance = new Lazy<NetworkTableInstance>(() =>
        {
            return new NetworkTableInstance(NtCore.GetDefaultInstance());
        });

        public static NetworkTableInstance Default => s_defaultInstance.Value;

        public static NetworkTableInstance Create()
        {
            var inst = new NetworkTableInstance(NtCore.CreateInstance());
            inst.m_owned = true;
            return inst;
        }

        public NT_Inst Handle { get; private set; }

        public NetworkTableEntry GetEntry(string name)
        {
            return new NetworkTableEntry(this, NtCore.GetEntry(Handle, name));
        }

        public NetworkTableEntry[] GetEntries(string prefix, NtType types)
        {
            return NtCore.GetEntriesManaged(this, prefix, types);
        }

        public EntryInfo[] GetEntryInfo(string prefix, NtType types)
        {
            return NtCore.GetEntryInfo(this, prefix, types);
        }

        private readonly ConcurrentDictionary<string, NetworkTable> m_tables = new ConcurrentDictionary<string, NetworkTable>();

        public NetworkTable GetTable(string key)
        {
            string theKey;
            if (string.IsNullOrWhiteSpace(key) || key.Equals(""))
            {
                theKey = "";
            } 
            else if (key[0] == NetworkTable.PathSeparator)
            {
                theKey = key;
            }
            else
            {
                theKey = NetworkTable.PathSeparator + key;
            }

            NetworkTable table = m_tables.GetOrAdd(theKey, (s) =>
            {
                return new NetworkTable(this, s);
            });
            return table;
        }

        public void DeleteAllEntries()
        {
            NtCore.DeleteAllEntries(Handle);
        }

        public void SetNetworkIdentity(string name)
        {
            NtCore.SetNetworkIdentity(Handle, name);
        }

        public NetworkMode GetNetworkMode()
        {
            return NtCore.GetNetworkMode(Handle);
        }

        public void StartServer(string persistFilename = "networktables.ini", 
                                string listenAddress = "", int port = DefaultPort)
        {
            NtCore.StartServer(Handle, persistFilename, listenAddress, port);
        }

        public void StopServer()
        {
            NtCore.StopServer(Handle);
        }

        public void StartClient()
        {
            NtCore.StartClient(Handle);
        }

        public void StartClient(string serverName, int port = DefaultPort)
        {
            NtCore.StartClient(Handle, serverName, port);
        }

        public void StartClient(ReadOnlySpan<string> serverNames, int port = DefaultPort)
        {
            Span<ServerPortPair> servers = new ServerPortPair[serverNames.Length];
            for (int i = 0; i < serverNames.Length; i++)
            {
                servers[i] = new ServerPortPair(serverNames[i], port);
            }
            StartClient(serverNames);
        }

        public void StartClient(ReadOnlySpan<ServerPortPair> servers)
        {
            NtCore.StartClient(Handle, servers);
        }

        public void StartClientTeam(int team, int port = DefaultPort)
        {
            NtCore.StartClientTeam(Handle, team, port);
        }

        public void StopClient()
        {
            NtCore.StopClient(Handle);
        }

        public void SetServer(string serverName, int port = DefaultPort)
        {
            NtCore.SetServer(Handle, serverName, port);
        }

        public void SetServer(ReadOnlySpan<string> serverNames, int port = DefaultPort)
        {
            Span<ServerPortPair> servers = new ServerPortPair[serverNames.Length];
            for (int i = 0; i < serverNames.Length; i++)
            {
                servers[i] = new ServerPortPair(serverNames[i], port);
            }
            SetServer(serverNames);
        }

        public void SetServer(ReadOnlySpan<ServerPortPair> servers)
        {
            NtCore.SetServer(Handle, servers);
        }

        public void SetServerTeam(int team, int port = DefaultPort)
        {
            NtCore.SetServerTeam(Handle, team, port);
        }

        public void StartDSClient(int port = DefaultPort)
        {
            NtCore.StartDSClient(Handle, port);
        }

        public void StopDSClient()
        {
            NtCore.StopDSClient(Handle);
        }

        public void SetUpdateRate(double interval)
        {
            NtCore.SetUpdateRate(Handle, interval);
        }

        public void Flush()
        {
            NtCore.Flush(Handle);
        }

        public ConnectionInfo[] GetConnections()
        {
            return NtCore.GetConnections(Handle);
        }

        public bool IsConnected()
        {
            return NtCore.IsConnected(Handle);
        }

        public void SavePersistent(string filename)
        {
            NtCore.SavePersistent(Handle, filename);
        }

        public List<string> LoadPersistent(string filename)
        {
            return NtCore.LoadPersistent(Handle, filename);
        }

        public void SaveEntries(string filename, string prefix)
        {
            NtCore.SaveEntries(Handle, filename, prefix);
        }

        public List<string> LoadEntries(string filename, string prefix)
        {
            return NtCore.LoadEntries(Handle, filename, prefix);
        }
    }
}