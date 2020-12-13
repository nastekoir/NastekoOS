using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Formatter;
namespace NastekoOS
{
    class Program
    {
        static string fsPath = @"C:\Users\naste\source\repos\NastekoOS\Formatter\bin\Debug\netcoreapp3.1\NastekoFS";
        static List<User> users = GetUsers();
        static List<Group> groups = GetGroups();
        static User currentUser;
        static Group currentGroup;
        static int userCount=GetUsers().Count;
        static int groupCount = GetGroups().Count;
        static FreeBlocks freeBlocks=new FreeBlocks() { FreeBlockList=new int[512]};
        static Inode[] freeInodes = new Inode[512];
        static string currentPath ="NastekoOS" + "/";
        static RootDirectory rootPath;
        static RootDirectory curPath;
        static RootDirectory[] notes = new RootDirectory[473];
        static RootDirectory prPath;
        static string preiousPath;
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в NastekoOS\n" +
                "Для помощи по командам используйте команду help");
            GetBlockList();
            GetInodeList();
            GetRootDirectory();
            rootPath = curPath;
            while (true)
            {
                Console.Write(currentPath);
                string cmd = Console.ReadLine();
                string[] parameters = cmd.Split(' ');
                switch (parameters[0])
                {
                    case "help":
                        help();
                        break;
                    case "enter":
                        if (parameters.Length == 3)
                        {
                            currentUser = enter(parameters[1], parameters[2]);
                            if (parameters[1] == "root")
                            {
                                curPath = rootPath.directories[0].directories[0];
                                currentPath = "NastekoOS/rootGroup/root/";
                            }
                            else
                            if (currentUser != null)
                            {
                                string currentGroupName = new string(currentGroup.GroupName).Trim('\0');
                                currentPath += "rootGroup/root/"+ currentGroupName + "/" + parameters[1] + "/";
                                curPath= rootPath.directories[0].directories[0].directories.Where(dir => new string(dir.Name).Trim() == new string(currentGroup.GroupName).Trim('\0')).First();
                                curPath = curPath.directories.Where(user => new string(user.Name).Trim() == new string(currentUser.Username).Trim('\0')).First();
                            }
                            
                        }
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "createuser":
                        if (parameters.Length == 4)
                        {
                            createuser(parameters[1], parameters[2],Convert.ToInt32(parameters[3]));
                            curPath = rootPath.directories[0].directories[0].directories[Convert.ToInt32(parameters[3])-2]; 
                            createdir(parameters[1]);
                        }
                            
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "deleteuser":
                        if (parameters.Length == 3)
                            deleteuser(parameters[1], parameters[2]);
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "creategroup":
                        if (parameters.Length == 2)
                        {
                            creategroup(parameters[1]);
                            createdir(parameters[1]);
                        }
                            
                            
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "deletegroup":
                        if (parameters.Length == 2)
                            deletegroup(parameters[1]);
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "createfile":
                        if(parameters.Length == 2)
                        {
                            createfile(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "writefile":
                        if (parameters.Length == 2)
                        {
                            writefile(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "readfile":
                        if (parameters.Length == 2)
                        {
                            readfile(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "deletefile":
                        if (parameters.Length == 2)
                        {
                            deletefile(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "createdir":
                        if (parameters.Length == 2)
                        {
                            createdir(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "deletedir":
                        if (parameters.Length == 2)
                        {
                            deletedir(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "changedir":
                        if (parameters.Length == 2)
                        {
                            changedir(parameters[1]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "rename":
                        if (parameters.Length == 3)
                        {
                            rename(parameters[1],parameters[2]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "move":
                        if (parameters.Length == 3)
                        {
                            move(parameters[1], parameters[2]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "chmod":
                        if (parameters.Length == 3)
                        {
                            chmod(parameters[1], parameters[2]);
                        }
                        else
                        {
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        }
                        break;
                    case "exit":
                        exit();
                        break;
                    case "ls":
                        GetFilesDirs();
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "off":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Ошибка синтаксиса, используйте команду help для помощи");
                        break;
                }
            }

        }

        static User enter(string username, string pass)
        {
            foreach (User user in users)
            {
                string name = new string(user.Username);
                string pas = new string(user.Password);
                if (username == name.Substring(0,  name.IndexOf("\0")) && pass == pas.Substring(0,pas.IndexOf("\0")))
                {
                    Console.WriteLine($"Вы вошли под пользователем {username}");
                    var curgroup = groups.Where(group => group.GroupID.Equals(user.GroupID)).Select(group => group);
                    currentGroup = curgroup.FirstOrDefault();
                    return user;
                    
                }
            }
            Console.WriteLine("Неверное имя пользователя или пароль");
            return null;
        }
        static void writefile(string filename)
        {
            
                RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == filename).First();
            if (freeInodes[file.InodeNumber].FileRights[6] == true)
            {
                Console.WriteLine("Файл доступен только для чтения");
            }
            else if ((currentUser.UserID== freeInodes[file.InodeNumber].UserID && freeInodes[file.InodeNumber].FileRights[1]==true)
                ||(currentUser.GroupID== freeInodes[file.InodeNumber].GroupID && freeInodes[file.InodeNumber].FileRights[4] == true))
            {
                Console.InputEncoding = Encoding.Unicode;
                string text = Console.ReadLine();
                text = UnicodeStr2HexStr(text);
                int num = freeBlocks.FreeBlockList[freeInodes[file.InodeNumber].BlockArray[0]];
                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(fsPath)))
                {
                    bw.BaseStream.Position = 78412 + 2048 * num;
                    bw.Write(text);
                }
            }
            else
                {
                Console.WriteLine("Недостаточно прав");
                }
            
        }
        static void readfile(string filename)
        {
            RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == filename).First();
                    int num = freeBlocks.FreeBlockList[freeInodes[file.InodeNumber].BlockArray[0]];
                    if((currentUser.UserID==freeInodes[file.InodeNumber].UserID&& freeInodes[file.InodeNumber].FileRights[0] == true)
                || (currentUser.GroupID == freeInodes[file.InodeNumber].GroupID && freeInodes[file.InodeNumber].FileRights[3] == true))
                        {
                            using (BinaryReader br = new BinaryReader(File.OpenRead(fsPath)))
                            {
                                br.BaseStream.Position = 78412 + 2048 * num;
                                Console.WriteLine(HexBytes2UnicodeStr(HexStr2HexBytes(br.ReadString())));
                            }
                        }
            else
            {
                Console.WriteLine("Недостаточно прав");
            }
        }
        static void rename(string oldname, string newname)
        {
            RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == oldname).First();
            int num = freeBlocks.FreeBlockList[freeInodes[file.InodeNumber].BlockArray[0]];
            while (newname.Length != 16)
            {
                newname += " ";
            }

            curPath.directories[curPath.directories.IndexOf(file)].Name = newname.ToCharArray();
            UpdateNotes();
        }
        static void exit()
        {
            if (currentUser != null)
            {
                string name = new string(currentUser.Username);
                Console.WriteLine($"Вы вышли из пользователя {name.Substring(0, name.IndexOf("\0"))}");
                currentUser = null;
                currentPath = "NastekoOS/";
                curPath = rootPath;
            }
            
        }
        static void createuser(string username, string pass, int groupid)
        {
            if (userCount == 100)
            {
                Console.WriteLine("Достигнут лимит пользователей 100");
                return;
            }
            if (username.Length > 16 || pass.Length > 16)
            {
                Console.WriteLine("Ошибка! Ограничение на Имя пользователя и пароль 32 символа");
                return;
            }
            foreach (User user in users)
            {
                string name = new string(user.Username);
                if (name.Substring(0, name.IndexOf("\0")) == username)
                {
                    Console.WriteLine("Такой пользователь уже существует");
                    return;
                }
            }
            using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(fsPath)))
            {
                User newUser = new User
                {
                    UserID = users[users.Count-1].UserID+1,
                    GroupID = groupid,
                    Username = username.ToCharArray(),
                    Password = pass.ToCharArray()
                };
                
                bw.BaseStream.Position = 49196;
                bw.BaseStream.Position += 72*userCount;
                bw.Write(newUser.GetBytes());
                
            }
            userCount++;
            users = GetUsers();
            Console.WriteLine($"Пользователь {username} создан");
        }

        static void deleteuser(string username, string pass)
        {
            if (username == "root")
            {
                Console.WriteLine("Вы не можете удалить root пользователя");
                return;
            }
            foreach (User user in users)
            {
                string name = new string(user.Username);
                string pas = new string(user.Password);
                
                if (username == name.Substring(0, name.IndexOf("\0")) && pass == pas.Substring(0, pas.IndexOf("\0")))
                {
                    RootDirectory tmpPath = curPath;
                    RootDirectory dir = curPath.directories[user.GroupID - 2];
                    curPath = dir;
                    RootDirectory dirPath=dir.directories.Where(dir => new string(dir.Name).Trim() == new string(user.Username).Trim('\0')).First();
                    deletedir(new string(dirPath.Name).Trim());
                    curPath = tmpPath;
                    users.Remove(user);
                    UpdateUsers();
                    users = GetUsers();
                    userCount = GetUsers().Count;
                    Console.WriteLine($"Пользователь {username} удалён");

                    
                    return;
                }
            }
            Console.WriteLine("Пользователь не найден");
        }
        static List<User> GetUsers()
        {
            List<User> users = new List<User>();

            char[] chars = new char[72];
            using (BinaryReader br = new BinaryReader(File.OpenRead(fsPath)))
            {
                br.BaseStream.Position = 49196;
                for (int i = 0; i < 100; i++)
                {
                    long tmpPos = br.BaseStream.Position;
                    if (br.ReadInt32() == 0)
                    {
                        return users;
                    }
                    br.BaseStream.Position = tmpPos;
                    User newUser = new User();
                    
                    newUser.UserID = br.ReadInt32();

                    chars = br.ReadChars(64);
                    for (int j = 0,k=0; j < 32; j++)
                    {
                        if (chars[j] != '\0')
                        {
                            newUser.Username[k] = chars[j];
                            k++;
                        }
                    }

                    for (int j = 32,k=0; j < 64; j++)
                    {
                        if (chars[j] != '\0')
                        {
                            newUser.Password[k] = chars[j];
                            k++;
                        }
                    }
                    
                    newUser.GroupID = br.ReadInt32();
                    users.Add(newUser);
                }
            }
            return users;
        }
        static void UpdateUsers()
        {
            using (BinaryWriter br = new BinaryWriter(File.OpenWrite(fsPath)))
            {
                br.BaseStream.Position = 49196;
                foreach (User user in users)
                {
                    br.BaseStream.Write(user.GetBytes());
                }
                while(br.BaseStream.Position!= 56396)
                {
                    br.BaseStream.Write(new User().GetBytes());
                }
            }
            
        }
        static void creategroup(string groupName)
        {
            if (currentUser.UserID != 1)
            {
                Console.WriteLine("Создавать группы может только root пользователь");
                return;
            }
            foreach (Group group in groups)
            {
                string name = new string(group.GroupName);
                if (name.Substring(0, name.IndexOf("\0")) == groupName)
                {
                    Console.WriteLine("Такая группа уже существует");
                    return;
                }
            }
           
            groups.Add(new Group()
            {
                GroupID = groups[groups.Count-1].GroupID+1,
                GroupName = groupName.ToCharArray()
            });
            UpdateGroups();
            groups = GetGroups();

            Console.WriteLine($"Группа {groupName} создана");
        }
        static void deletegroup(string groupName)
        {
            if (currentUser.UserID != 1)
            {
                Console.WriteLine("Удалять группы может только root пользователь");
                return;
            }
            if (groupName == "rootGroup")
            {
                Console.WriteLine("Невозможно удалить rootGroup");
            }
            Console.WriteLine("При удалении группы, удалятся все пользовтели группы!\nУдалить группу? y/n");
            int key = Console.ReadKey().KeyChar;
            switch (key)
            {
                case 121:
                    break;
                case 110:
                    return;
                default:
                    return;
            }
            foreach (Group group in groups)
            {
                string name = new string(group.GroupName);
                
                var tmpListUsers = new List<User>(users);
                if (name.Substring(0, name.IndexOf("\0")) == groupName)
                {
                    for (int i=0;i<tmpListUsers.Count;i++)
                    {
                        string nameUser = new string(tmpListUsers[i].Username);
                        string pas = new string(tmpListUsers[i].Password);
                        if (tmpListUsers[i].GroupID == group.GroupID)
                        {
                            deleteuser(nameUser.Substring(0, nameUser.IndexOf("\0")), pas.Substring(0, pas.IndexOf("\0")));
                        }
                    }
                    RootDirectory dirname = curPath.directories.Where(dir => new string(dir.Name).Trim() == new string(group.GroupName).Trim('\0')).First();
                    deletedir(new string(dirname.Name).Trim());
                    groups.Remove(group);
                    UpdateGroups();
                    groups = GetGroups();
                    groupCount = GetGroups().Count;
                    Console.WriteLine($"Группа {groupName} удалена");
                    return;
                }
            }

            Console.WriteLine("Группа не найдена");

        }
        static List<Group> GetGroups()
        {
            List<Group> groups = new List<Group>();
            char[] chars = new char[36];
            using(BinaryReader br=new BinaryReader(File.OpenRead(fsPath)))
            {
                br.BaseStream.Position = 56396;
                for (int i = 0; i < 20; i++)
                {
                    long tmpPos = br.BaseStream.Position;
                    if (br.ReadInt32() == 0)
                    {
                        return groups;
                    }
                    br.BaseStream.Position = tmpPos;
                    Group newGroup = new Group();
                    newGroup.GroupID=br.ReadInt32();
                    chars=br.ReadChars(32);
                    
                    for (int j = 0, k = 0; j < 32; j++)
                    {
                        if (chars[j] != '\0')
                        {
                            newGroup.GroupName[k] = chars[j];
                            k++;
                        }
                    }
                    groups.Add(newGroup);
                }
            }
            return groups;
        }
        static void UpdateGroups()
        {
            using (BinaryWriter br = new BinaryWriter(File.OpenWrite(fsPath)))
            {
                br.BaseStream.Position = 56396;
                foreach (Group group in groups)
                {
                    br.BaseStream.Write(group.GetBytes());
                }
                while (br.BaseStream.Position != 57116)
                {
                    br.BaseStream.Write(new Group().GetBytes());
                }
            }
        }

        static void createfile(string filename)
        {
            
            foreach (RootDirectory f in curPath.directories)
            {
                if(new string(f.Name).Trim() == filename)
                {
                    Console.WriteLine("Такой файл существует");
                    return;
                }
            }
            
            char[] date = new char[10];
            date = DateTime.Now.ToString().Take(10).ToArray();
            int freeInodeIndex = GetFreeInode();

            Inode inode = new Inode()
            {
                FileSize = 2048,
                FileRights = new bool[] { true, true, true, true, true, true, false, false },
                UserID = currentUser.UserID,
                GroupID = currentUser.GroupID,
                Date =new char[] { date[0],date[1], date[2], date[3], date[4], date[5], date[6], date[7], date[8], date[9] },
                BlockArray =new int[] {GetFreeBlock(),0,0,0,0,0,0,0,0,0,0,0,0}
            };
            freeBlocks.FreeBlockList[GetFreeBlock() - 1] = 0;
            freeInodes[freeInodeIndex] =inode;
            UpdateBlocks();
            UpdateInodes();
            GetBlockList();
            GetInodeList();
            string spaces = "";
            for(int i = 0; i < 16 - filename.Length; i++)
            {
                spaces += " ";
            }
            RootDirectory file = new RootDirectory()
            {
                Name = (filename+spaces).ToCharArray(),
                Extention = new char[] { '.', 't', 'x', 't' },
                InodeNumber = freeInodeIndex,
            };
            curPath.directories.Add(file);
            UpdateNotes();

        }

        static void deletefile(string filename)
        {
            RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == filename).First();

            //note.Name = new char[16];
            //note.InodeNumber = 0;
            freeBlocks.FreeBlockList[freeInodes[file.InodeNumber].BlockArray[0]-1]= freeInodes[file.InodeNumber].BlockArray[0];
            freeInodes[file.InodeNumber] = new Inode();
            curPath.directories[curPath.directories.IndexOf(file)].InodeNumber = 0;
            curPath.directories.Remove(file);
            UpdateBlocks();
            UpdateInodes();   
            UpdateNotes();
        }
        static void chmod(string filename, string rights)
        {
            RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == filename).First();
            char[] right = rights.ToCharArray();
            
            for (int i = 0; i < right.Length; i++)
            {
                if (right[i] == '-')
                {
                    freeInodes[file.InodeNumber].FileRights[i] = false;
                }
                else
                {
                    freeInodes[file.InodeNumber].FileRights[i] = true;
                }
            }
            UpdateInodes();
        }
        static void UpdateInodes()
        {
            using (BinaryWriter bw=new BinaryWriter(File.OpenWrite(fsPath)))
            {
                bw.BaseStream.Position = 2092;
                foreach (Inode inode in freeInodes)
                {
                    bw.BaseStream.Write(inode.GetBytes());
                }
            }
        }
        static int GetFreeInode()
        {
            for (int i = 0; i < freeInodes.Length; i++)
            {
                if (freeInodes[i].FileSize == 0)
                {
                    return i;
                }
            }
            Console.WriteLine("Диск заполнен");
            return 0;
        }
        static void GetInodeList()
        {
            using (BinaryReader br=new BinaryReader(File.OpenRead(fsPath)))
            {
                br.BaseStream.Position = 2092;
                for (int i=0;i<freeInodes.Length;i++)
                {
                    Inode inode = new Inode();
                    inode.FileSize = br.ReadInt32();
                    inode.FileRights = new bool[] { br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean(), br.ReadBoolean() };
                    inode.UserID = br.ReadInt32();
                    inode.GroupID = br.ReadInt32();
                    char[] date = new char[20];
                    date = br.ReadChars(20);
                    for (int j = 0,k=0; j < 20; j++)
                    {
                        if(date[j]!='\0')
                        {
                            inode.Date[k] = date[j];
                            k++;
                        }
                        
                    }
                    int[] blocks= br.ReadBytes(52).Select(x => (int)x).ToArray();
                    Array.Resize<int>(ref blocks, 13);
                    inode.BlockArray = blocks;
                    freeInodes[i] = inode;
                }
                
            }
        }
        static int GetFreeBlock()
        {
            for (int i = 0; i < freeBlocks.FreeBlockList.Length; i++)
            {
                if (freeBlocks.FreeBlockList[i] != 0)
                {
                    return freeBlocks.FreeBlockList[i];
                }
            }
            Console.WriteLine("Диск заполнен, удалите ненужные файлы");
            return 0;
        }
        static void UpdateBlocks()
        {
            using(BinaryWriter bw=new BinaryWriter(File.OpenWrite(fsPath)))
            {
                bw.BaseStream.Position = 44;
                bw.Write(freeBlocks.GetBytes());
            }
        }
        static void GetBlockList()
        {   
            using(BinaryReader br=new BinaryReader(File.OpenRead(fsPath)))
            {
                br.BaseStream.Position = 44;
                for (int i = 0; i < 512; i++)
                {
                    freeBlocks.FreeBlockList[i] = br.ReadInt32();
                }
            }
        }
        static void createdir(string dirname)
        {
            while (dirname.Length != 16)
            {
                dirname += " ";
            }
            char[] date = new char[10];
            date = DateTime.Now.ToString().Take(10).ToArray();
            int freeInode = GetFreeInode();
            Inode inode = new Inode()
            {
                FileSize = 2048,
                FileRights = new bool[] { true, true, true, true, true, true, false, true },
                UserID = currentUser.UserID,
                GroupID = currentUser.GroupID,
                Date = new char[] { date[0], date[1], date[2], date[3], date[4], date[5], date[6], date[7], date[8], date[9] },
                BlockArray = new int[] { GetFreeBlock(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            };
            RootDirectory dir = new RootDirectory()
            {
                Name = dirname.ToCharArray(),
                Extention = new char[] { ' ', ' ', ' ', ' ' },
                InodeNumber = GetFreeInode(),
            };
            freeInodes[freeInode] = inode;
            curPath.directories.Add(dir);
            UpdateInodes();
            UpdateNotes();
        }
        static void deletedir(string dirname)
        {
            RootDirectory dir = curPath.directories.Where(dir => new string(dir.Name).Trim() == dirname).First();
            RootDirectory tmpDir = curPath;
            for (int i = dir.directories.Count-1; i >= 0; i--)
            {
                
                curPath = dir;
                if (freeInodes[dir.directories[i].InodeNumber].FileRights[7] == true)
                {
                    deletedir(new string(dir.directories[i].Name).Trim());
                }
                else
                {
                    deletefile(new string(dir.directories[i].Name).Trim());
                }
            }

            curPath = tmpDir;
            //freeBlocks.FreeBlockList[freeInodes[dir.InodeNumber].BlockArray[0] - 1] = freeInodes[dir.InodeNumber].BlockArray[0];
            freeInodes[dir.InodeNumber] = new Inode();
            curPath.directories[curPath.directories.IndexOf(dir)].InodeNumber = 0;
            curPath.directories.Remove(dir);
            UpdateBlocks();
            UpdateInodes();
            UpdateNotes();

        }
        static void changedir(string dirname)
        {
            if (dirname == "..")
            {
                curPath = prPath;
                currentPath = preiousPath;
            }
            else if (dirname=="/")
            {
                curPath = rootPath;
                currentPath = "Nasteko/";
            }
            else
            {
                prPath = curPath;
                preiousPath = currentPath;
                curPath = curPath.directories.Where(dir => new string(dir.Name).Trim() == dirname).First();
                currentPath += dirname + "/";
            }
            
        }
        static void move(string filename,string path)
        {
            RootDirectory file = curPath.directories.Where(file => new string(file.Name).Trim() == filename).First();
            curPath.directories.Remove(file);
            
            changedir(path);
            curPath.directories.Add(file);
            //changedir("..");
            UpdateNotes();
        }
        static void UpdateNotes()
        {
                using(BinaryWriter bw=new BinaryWriter(File.OpenWrite(fsPath)))
                {
                    bw.BaseStream.Position = 57116;
                    bw.Write(rootPath.GetBytes());
                }
            
        }
        static void GetRootDirectory()
        {
            using(BinaryReader br=new BinaryReader(File.OpenRead(fsPath)))
            {
                br.BaseStream.Position = 57116;
                
                    
                    RootDirectory note = new RootDirectory();
                    char[] name = br.ReadChars(32);
                    char[] ext = br.ReadChars(8);
                    for (int i = 0, k = 0; i < 32; i++)
                    {
                        if (name[i] != '\0')
                        {
                            note.Name[k] = name[i];
                            k++;
                        }
                    }
                    for (int i = 0, k = 0; i < 8; i++)
                    {
                        if (ext[i] != '\0')
                        {
                            note.Extention[k] = ext[i];
                            k++;
                        }
                    }
                    note.InodeNumber = br.ReadInt32();
                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();
                    note.directories=binFormatter.Deserialize(br.BaseStream) as List<RootDirectory>;
                curPath = note;
                }
                
            }
        static void GetFilesDirs()
        {
            foreach (RootDirectory directory in curPath.directories)
            {
                Console.Write(new string(directory.Name).Trim());
                char[] rights = new char[] { 'r', 'w', 'x', 'r', 'w', 'x', 'R', 'd' };
                Console.Write(' ');
                
                
                for (int i = 0; i < freeInodes[directory.InodeNumber].FileRights.Length; i++)
                {

                    if (freeInodes[directory.InodeNumber].FileRights[i] == true)
                    {
                        Console.Write(rights[i]);
                    }
                    else
                    {
                        Console.Write('-');
                    }
                }
                Console.WriteLine();
                
                
            }
            
        }
        static void help()
        {
            using(StreamReader sr=new StreamReader(File.OpenRead("help.txt")))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
        public static String UnicodeStr2HexStr(String strMessage)
        {
            byte[] ba = Encoding.BigEndianUnicode.GetBytes(strMessage);
            String strHex = BitConverter.ToString(ba);
            strHex = strHex.Replace("-", "");
            return strHex;
        }

        public static String HexStr2UnicodeStr(String strHex)
        {
            byte[] ba = HexStr2HexBytes(strHex);
            return HexBytes2UnicodeStr(ba);
        }

        
        public static String HexBytes2UnicodeStr(byte[] ba)
        {
            var strMessage = Encoding.BigEndianUnicode.GetString(ba, 0, ba.Length);
            return strMessage;
        }

        public static byte[] HexStr2HexBytes(String strHex)
        {
            strHex = strHex.Replace(" ", "");
            int nNumberChars = strHex.Length / 2;
            byte[] aBytes = new byte[nNumberChars];
            using (var sr = new StringReader(strHex))
            {
                for (int i = 0; i < nNumberChars; i++)
                    aBytes[i] = Convert.ToByte(new String(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return aBytes;
        }
    }
}
