using System;
using System.Collections.Generic;
using System.IO;
using Formatter;
namespace NastekoOS
{
    class Program
    {
        static string fsPath = @"C:\Users\naste\source\repos\NastekoOS\Formatter\bin\Debug\netcoreapp3.1\NastekoFS";
        static List<User> users = GetUsers();
        static List<Group> groups = GetGroups();
        static User currentUser;
        static int userCount=GetUsers().Count;
        static int groupCount = GetGroups().Count;
        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в NastekoOS\n" +
                "Для помощи по командам используйте команду help");
            while (true)
            {
                string cmd = Console.ReadLine();
                string[] parameters = cmd.Split(' ');
                switch (parameters[0])
                {
                    case "help":
                        help();
                        break;
                    case "enter":
                        if (parameters.Length == 3)
                            currentUser = enter(parameters[1], parameters[2]);
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "createuser":
                        if (parameters.Length == 3)
                            createuser(parameters[1], parameters[2]);
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
                            creategroup(parameters[1]);
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "deletegroup":
                        if (parameters.Length == 2)
                            deletegroup(parameters[1]);
                        else
                            Console.WriteLine("Неверное количество параметров, используйте команду help для помощи");
                        break;
                    case "exit":
                        exit();
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
                    return user;
                }
            }
            Console.WriteLine("Неверное имя пользователя или пароль");
            return null;
        }

        static void exit()
        {
            if (currentUser != null)
            {
                string name = new string(currentUser.Username);
                Console.WriteLine($"Вы вышли из пользователя {name.Substring(0, name.IndexOf("\0"))}");
                currentUser = null;
            }
            
        }
        static void createuser(string username, string pass)
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
                    GroupID = 2,
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

        static void help()
        {
            
        }
    }
}
