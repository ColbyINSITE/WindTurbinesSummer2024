/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderFileAsync
    {

        private const int BUFFER_SIZE = 0x4096;

        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true);
        }
        public static FileStream OpenWrite(string path)
        {
            // Open a file stream for writing and that supports asynchronous I/O
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BUFFER_SIZE, true);
        }
        public static async Task<byte[]> ReadAllBytes(string path)
        {
            using (var fs = OpenRead(path))
            {
                var buff = new byte[fs.Length];
                await fs.ReadAsync(buff, 0, (int)fs.Length);
                return buff;
            }
        }
        public static async Task WriteAllBytes(string path, byte[] bytes)
        {
            if (path == null) throw new ArgumentException("path");
            if (bytes == null) throw new ArgumentException("bytes");

            using (var fs = OpenWrite(path))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        public static async Task<string> ReadAllText(string path, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            using (var reader = new StreamReader(path, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
        public static async Task WriteAllText(string path, string contents, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            await WriteAllBytes(path, encoding.GetBytes(contents));
        }
        public static async Task<string[]> ReadAllLines(string path, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var lines = new List<string>();
            using (var reader = new StreamReader(path, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    lines.Add(line);
            }
            return lines.ToArray();

        }
        public static async Task Copy(string sourceFileName, string destFileName)
        {
            using (var sourceStream = File.Open(sourceFileName, FileMode.Open))
            {
                using (var destinationStream = File.Create(destFileName))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }
        public static async Task Move(string sourceFileName, string destFileName)
        {
            using (var sourceStream = File.Open(sourceFileName, FileMode.Open))
            {
                using (var destinationStream = File.Create(destFileName))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }

            File.Delete(sourceFileName);
        }
    }
}