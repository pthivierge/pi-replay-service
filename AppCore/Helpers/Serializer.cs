#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PIReplay.Core.PISystemHelpers
{
    public static class Serializer
    {
        public static void SerializeObject(string filename, Object objectToSerialize)
        {
            using (Stream stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
                stream.Close();
            }
        }

        public static object DeSerializeObject(string filename)
        {
            object objectToSerialize;
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                var bFormatter = new BinaryFormatter();
                objectToSerialize = bFormatter.Deserialize(stream);
                stream.Close();
                return objectToSerialize;
            }
        }
    }
}
