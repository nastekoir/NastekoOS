using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Formatter
{
    public class FileSystemStructure
    {
        public FileSystemStructure()
        {
            int diskSize= 1048576;
            Superblock superblock = new Superblock();
            FreeBlocks freeBlocks = new FreeBlocks();
            for (int i = 0; i < freeBlocks.FreeBlockList.Length; i++)
            {
                if (i < 28)
                {
                    freeBlocks.FreeBlockList[i] = 0;
                }
                else
                {
                    freeBlocks.FreeBlockList[i] = i + 1;
                }
                
            }
            
            Inode[] inodes = new Inode[512];
            for(int i=0;i<inodes.Length;i++)
            {
                inodes[i] = new Inode();
            }
            User[] users = new User[100];
            Group[] groups = new Group[20];
            
            users[0] = new User()
            {
                UserID = 1,
                Username = "root".ToCharArray(),
                Password = "toor".ToCharArray(),
                GroupID = 1
            };
            for (int i = 1; i < users.Length; i++)
            {
                users[i] = new User();
            }
            groups[0] = new Group()
            {
                GroupID = 1,
                GroupName = "rootGroup".ToCharArray()
            };
            for (int i = 1; i < groups.Length; i++)
            {
                groups[i] = new Group();
            }

            RootDirectory rootDirectory = new RootDirectory()
            {
                Name = "NastekoOS       ".ToCharArray(),
                Extention = new char[] { ' ', ' ', ' ', ' ' },
                InodeNumber = 1,
                directories = new List<RootDirectory>(),
            };
            rootDirectory.directories.Add(new RootDirectory()
            {
                Name = "rootGroup       ".ToCharArray(),
                Extention = new char[] { ' ', ' ', ' ', ' ' },
                InodeNumber = 1,
            });
            rootDirectory.directories[0].directories.Add(new RootDirectory()
            {
                Name = "root            ".ToCharArray(),
                Extention = new char[] { ' ', ' ', ' ', ' ' },
                InodeNumber = 2,
            });
            

            FileInfo fileInfo = new FileInfo("NastekoFS");
            using(BinaryWriter bw=new BinaryWriter(fileInfo.Create(), Encoding.Unicode))
            {
                bw.Write(superblock.GetBytes());
                bw.Write(freeBlocks.GetBytes());
                int inodesSize = 0;
                int usersSize = 0;
                int groupsSize = 0;
                foreach (Inode inode in inodes)
                {
                    bw.Write(inode.GetBytes());
                    inodesSize += inode.GetBytes().Length;
                }
                foreach (User user in users)
                {
                    bw.Write(user.GetBytes());
                    usersSize += user.GetBytes().Length;
                }
                foreach (Group group in groups)
                {
                    bw.Write(group.GetBytes());
                    groupsSize += group.GetBytes().Length;
                }
                
                bw.Write(rootDirectory.GetBytes());
                
                
                bw.Write(new byte[diskSize - superblock.GetBytes().Length - freeBlocks.GetBytes().Length - inodesSize - usersSize - groupsSize - rootDirectory.GetBytes().Length]);
            }
            Console.WriteLine("Форматирование завершено!");
        }
    }
    class Superblock
    {
        int BlockSize=2048;
        int BlockFree=512;
        char[] SysType = "NastekoOSfileSys".ToCharArray();
        int InodeArrSize=512;
        public byte[] GetBytes()
        {
            byte[] data = new byte[44];
            int index = 0;
            foreach(char c in SysType)
            {
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += 2;
            }
            
            
            BitConverter.GetBytes(BlockSize).CopyTo(data, 32);
            BitConverter.GetBytes(BlockFree).CopyTo(data, 36);
            BitConverter.GetBytes(InodeArrSize).CopyTo(data, 40);
            return data;
        }
    }
    public class FreeBlocks
    {
        public int[] FreeBlockList = new int[512];
        
        public byte[] GetBytes()
        {
            byte[] data = new byte[2048];
            int index = 0;
            foreach (int block in FreeBlockList)
            {
                BitConverter.GetBytes(block).CopyTo(data, index);
                index += 4;
            }
            return data;
        }
    }
    
    public class Inode
    {
        public int FileSize;
        public bool[] FileRights = new bool[8];
        public int UserID;
        public int GroupID;
        public char[] Date = new char[10];
        public int[] BlockArray = new int[13];
        public byte[] GetBytes()
        {
            byte[] data = new byte[92];
            int index = 0;
            BitConverter.GetBytes(FileSize).CopyTo(data, index);
            index += BitConverter.GetBytes(FileSize).Length;
            foreach (bool right in FileRights)
            {
                BitConverter.GetBytes(right).CopyTo(data, index);
                index += BitConverter.GetBytes(right).Length;
            }
            BitConverter.GetBytes(UserID).CopyTo(data, index);
            index += BitConverter.GetBytes(UserID).Length;
            BitConverter.GetBytes(GroupID).CopyTo(data, index);
            index += BitConverter.GetBytes(GroupID).Length;
            foreach (char c in Date)
            {
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            foreach (int block in BlockArray)
            {
                BitConverter.GetBytes(block).CopyTo(data, index);
                index += BitConverter.GetBytes(block).Length;
            }
            return data;
        }
    }
    public class User
    {
        public int UserID;
        public char[] Username = new char[16];
        public char[] Password = new char[16];
        public int GroupID;
        public byte[] GetBytes()
        {
            byte[] data = new byte[72];
            int index = 0;
            BitConverter.GetBytes(UserID).CopyTo(data, index);
            index += BitConverter.GetBytes(UserID).Length;
            int tmp = 0;
            foreach (char c in Username)
            {
                tmp += 2;
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            index += 32 - tmp;
            tmp = 0;
            foreach (char c in Password)
            {
                tmp += 2;
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            index += 32 - tmp;
            BitConverter.GetBytes(GroupID).CopyTo(data, index);
            return data;
        }
    }

    public class Group
    {
        public int GroupID;
        public char[] GroupName = new char[16];
        public byte[] GetBytes()
        {
            byte[] data = new byte[36];
            int index = 0;
            BitConverter.GetBytes(GroupID).CopyTo(data, index);
            index += BitConverter.GetBytes(GroupID).Length;
            foreach (char c in GroupName)
            { 
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            return data;
        }
    }
    [Serializable]
    public class RootDirectory
    {
        public char[] Name = new char[16];
        public char[] Extention = new char[4];
        public int InodeNumber=0;
        public List<RootDirectory> directories = new List<RootDirectory>();
        public byte[] GetBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, directories);
            byte[] data = new byte[44+ms.Length];
            int index = 0;
            foreach (char c in Name)
            {
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            foreach (char c in Extention)
            {
                BitConverter.GetBytes(c).CopyTo(data, index);
                index += BitConverter.GetBytes(c).Length;
            }
            BitConverter.GetBytes(InodeNumber).CopyTo(data, index);
            index += BitConverter.GetBytes(InodeNumber).Length;
            ms.ToArray().CopyTo(data, index);
            
            
            return data;
        }
    }
}
