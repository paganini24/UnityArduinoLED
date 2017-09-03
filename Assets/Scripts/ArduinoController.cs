﻿using UnityEngine;
using System.IO.Ports;
using System.Collections;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

namespace ArduinoUnity
{
    public class ArduinoController : NetworkBehaviour
    {
        [SerializeField]
        protected string portName = "COM3"; // changes on MAC check from Arduino Ide

        [SerializeField]
        private bool _openPortOnStart;

        [SerializeField]
        private bool _listenPrint;

        [SerializeField]
        private GameObject _canvasPort;
        [SerializeField]
        private InputField _inputPort;

        SerialPort stream;
        bool _isListening;
        string dataString = null;

        public string PortName
        {
            get { return portName; }
            set { portName = value; }
        }

        void Start()
        {
            _canvasPort.SetActive(isServer);
            if (!isServer)
                enabled = false;
            _inputPort.text = portName;
            if (_openPortOnStart)
                OpenPort();
        }

        void OnDisable()
        {
            if (stream != null && stream.IsOpen)
                stream.Close();
        }

        public void OpenPort()
        {
            stream = new SerialPort(portName, 9600);
            try
            {
                if (!stream.IsOpen)
                    stream.Open();
                stream.ReadTimeout = 1;
                stream.WriteTimeout = 50;

                print("Port opened : " + stream.IsOpen);
                _canvasPort.SetActive(false);
            }
            catch(IOException ex)
            {
                Debug.LogError("Error occured while opening port: "+ex.Message);
                _inputPort.image.color = Color.red;
                _canvasPort.SetActive(true);
            }
        }

        public void WriteToArduino(string message)
        {
            if (stream == null || !stream.IsOpen ) return;
            stream.WriteLine(message);
            stream.BaseStream.Flush();
        }

        private void Update()
        {
            if (stream == null || !stream.IsOpen) return;
            if (_listenPrint && !_isListening)
                StartCoroutine(AsynchronousReadFromArduino(OnStreamReceived, null, 5f));
        }

        private void OnStreamReceived(string obj)
        {
            Debug.Log("Arduino : "+obj);
        }

        private IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
        {
            _isListening = true;
            DateTime initialTime = DateTime.Now;
            DateTime nowTime;
            TimeSpan diff = default(TimeSpan);
            do
            {
                try
                {
                    dataString = stream.ReadLine();
                }
                catch (TimeoutException)
                {
                    dataString = null;
                }

                if (!string.IsNullOrEmpty(dataString))
                {
                    callback(dataString);
                    yield return null;
                }
                else
                    yield return new WaitForSeconds(0.05f);

                nowTime = DateTime.Now;
                diff = nowTime - initialTime;

            } while (diff.Milliseconds < timeout);

            if (fail != null)
                fail();
            yield return null;
            _isListening = false;
        }

    } 
}
