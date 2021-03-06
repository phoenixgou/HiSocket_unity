﻿//****************************************************************************
// Description:tcp socket and message regist
// Author: hiramtan@live.com
//****************************************************************************

using System;
using UnityEngine;
using HiSocket;
using System.Collections.Generic;

public class TestTcp2 : MonoBehaviour
{
    private ITcp _tcp;
    private IPackage _packer = new Packer();
    // Use this for initialization
    void Start()
    {
        RegistMsg();
        Init();
    }
    void Update()
    {
        _tcp.Run();
    }

    void Init()
    {
        _tcp = new TcpConnection(_packer);
        _tcp.StateChangeEvent += OnState;
        _tcp.ReceiveEvent += OnReceive;
        Connect();
    }
    void Connect()
    {
        _tcp.Connect("127.0.0.1", 7777);
    }
    void OnState(SocketState state)
    {
        Debug.Log("current state is: " + state);
        Debug.Log("current state is: " + state);
        if (state == SocketState.Connected)
        {
            Debug.Log("connect success");
        }
        else if (state == SocketState.DisConnected)
        {
            Debug.Log("connect failed");
        }
        else if (state == SocketState.Connecting)
        {
            Debug.Log("connecting");
        }
    }
    void Send()
    {
        for (int i = 0; i < 10; i++)
        {
            var bytes = BitConverter.GetBytes(i);
            Debug.Log("send message: " + i);
            _tcp.Send(bytes);
        }
    }
    private void OnApplicationQuit()
    {
        _tcp.DisConnect();
    }
    void OnReceive(byte[] bytes)
    {
        //Debug.Log("receive bytes: " + BitConverter.ToInt32(bytes, 0));
        var byteArray = new ByteArray();
        byteArray.Write(bytes);
        MsgRegister.Dispatch("10001", byteArray);
    }
    public class Packer : IPackage
    {
        private bool _isGetHead = false;
        private int _bodyLength;
        public void Unpack(IByteArray reader, Queue<byte[]> receiveQueue)
        {
            if (!_isGetHead)
            {
                if (reader.Length >= 2)//2 is example, get msg's head length
                {
                    var bodyLengthBytes = reader.Read(2);
                    _bodyLength = BitConverter.ToUInt16(bodyLengthBytes, 0);
                }
                else
                {
                    if (reader.Length >= _bodyLength)//get body
                    {
                        var bytes = reader.Read(_bodyLength);
                        receiveQueue.Enqueue(bytes);
                        _isGetHead = false;
                    }
                }
            }
        }
        public void Pack(Queue<byte[]> sendQueue, IByteArray writer)
        {
            var bytesWaitToPack = sendQueue.Dequeue();
            UInt16 length = (UInt16)bytesWaitToPack.Length;//get head lenth
            var bytesHead = BitConverter.GetBytes(length);
            writer.Write(bytesHead);//write head
            writer.Write(bytesWaitToPack);//write body
        }
    }


    #region receive message
    void RegistMsg()
    {
        MsgRegister.Regist("10001", OnMsg_Bytes);
        MsgRegister.Regist("10002", OnMsg_Protobuf);
    }

    void OnMsg_Bytes(IByteArray byteArray)
    {
        var msg = new MsgBytes(byteArray);
        int getInt = msg.Read<int>();
    }

    void OnMsg_Protobuf(IByteArray byteArray)
    {
        var msg = new MsgProtobuf(byteArray);
        GameObject testClass = msg.Read<GameObject>();//your class's type
        var testName = testClass.name;
    }
    #endregion
    #region send message
    void Msg_Bytes()
    {
        var msg = new MsgBytes();
        int x = 10;
        msg.Write(x);
        byte[] bytes = msg.ByteArray.Read(msg.ByteArray.Length);
        _tcp.Send(bytes);
    }
    void Msg_Protobuf()
    {
        var msg = new MsgProtobuf();
        var testGo = new GameObject();
        msg.Write(testGo);
        byte[] bytes = msg.ByteArray.Read(msg.ByteArray.Length);
        _tcp.Send(bytes);
    }
    #endregion
}