﻿#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using BridgeVs.Grapple.Contracts;
using BridgeVs.Grapple.Extensions;
using BridgeVs.Grapple.Grapple;
using BridgeVs.Locations;
using BridgeVs.Logging;

namespace BridgeVs.Grapple
{
    /// <summary>
    /// 
    /// </summary>
    public class Truck : ITruck
    {
        private readonly string _truckName;
        private readonly IGrapple _grapple;
        private Dictionary<Type, List<byte[]>> _container = new Dictionary<Type, List<byte[]>>();

        private string TruckPosition => Path.Combine(CommonFolderPaths.GrappleFolder, _truckName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truckName"></param>
        public Truck(string truckName)
            : this(truckName, new BinaryGrapple())
        {
        }

        internal Truck(string truckName, IGrapple grapple)
        {
            Log.Write($"Setting Up the Truck {truckName}");

            _truckName = truckName;
            _grapple = grapple;

            if (Directory.Exists(TruckPosition)) return;

            //no need for security access
            Directory.CreateDirectory(TruckPosition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadCargo<T>(T item)
        {
            Log.Write("Loading Cargo");
            Tuple<Type, byte[]> serializedType = _grapple.Grab(item);

            Type typeCodeName = serializedType.Item1;
            //If the type is already in and the same object has not been already added then add to the internal map
            if (!_container.ContainsKey(typeCodeName))
                _container.Add(typeCodeName, new List<byte[]> { serializedType.Item2 });
            else
                _container[typeCodeName].Add(serializedType.Item2);

            Log.Write("Cargo Loaded");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool DeliverTo(string address)
        {
            Log.Write($"Delivering Cargo to {address}");

            try
            {
                Tuple<Type, byte[]> buffer = _grapple.Grab(_container);

                using (MemoryMappedFile ipcMappedFile = MemoryMappedFile.CreateFromFile(
                    new FileStream(Path.Combine(TruckPosition, address), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
                    , _truckName + address
                    , buffer.Item2.Length
                    , MemoryMappedFileAccess.ReadWrite
                    , null
                    , HandleInheritability.Inheritable
                    , false))
                {
                    using (MemoryMappedViewStream stream = ipcMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.ReadWrite))
                    {
                        stream.Write(buffer.Item2, 0, buffer.Item2.Length);
                    }
                }
                Log.Write($"Cargo Successfully Delivered {address}");

                return true;
            }
            catch (Exception e)
            {
                Log.Write(e, "Error During DeliveryTo");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> UnLoadCargo<T>()
        {
            Log.Write("UnLoading Cargo of Type {0}", typeof(T).FullName);

            Type typeCodeName = typeof(T);
            return (from i in _container.Keys
                    where typeCodeName.IsAssignableFrom(i)
                    let objects = _container[i]
                    select objects.Select(o => _grapple.Release<T>(o)))
                    .SelectMany(o => o);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<object> UnLoadCargo(Type type)
        {
            Log.Write("Unstuffing Cargo of Type {0}", type.FullName);

            return (from i in _container.Keys
                    where type.IsAssignableFrom(i)
                    let objects = _container[i]
                    select objects.Select(o => _grapple.Release(o, type)))
                   .SelectMany(o => o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public Task WaitDelivery(string address, int timeout = 5000)
        {
            try
            {
                return Task.Factory.StartNew(() => WaitForNextTruck(address, timeout));
            }
            catch (AggregateException ae)
            {
                ae.Handle(exception =>
                {
                    Log.Write(exception, "Error Waiting Delivery");
                    return false;
                });
                throw ae.Flatten();
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Error Waiting Truck");
                throw;
            }
        }

        private void WaitForNextTruck(string address, int timeout)
        {
            const int waiting = 100;

            if (timeout == 0) return;

            try
            {
                byte[] buffer = InternalCollect(address);

                if (buffer.Length == 0)
                {
                    Thread.Sleep(waiting);
                    WaitForNextTruck(address, timeout - waiting);
                }
                else
                    //Container is being merged with the current loaded
                    _container = _grapple
                        .Release<Dictionary<Type, List<byte[]>>>(buffer)
                        .MergeLeft(_container);

            }
            catch (Exception e)
            {
                Log.Write(e, "Error Waiting Truck. Truck doesn't exist");
                throw;
            }
        }

        private byte[] InternalCollect(string address)
        {
            string filePath = Path.Combine(TruckPosition, address);
            if (!File.Exists(filePath)) return new byte[0];

            using (MemoryMappedFile ipcMappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open))
            {
                using (MemoryMappedViewStream stream = ipcMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.ReadWrite))
                {
                    byte[] @byte = new byte[stream.Length];
                    stream.Read(@byte, 0, (int)stream.Length);
                    return @byte;
                }
            }
        }
    }
}