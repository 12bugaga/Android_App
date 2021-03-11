using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace App3
{
    static class StartProgram
    {
        static private void SeparateImage(string pathToImage)
        {
            int countPart = 3;
            string stringImage = WorkWirhImages.GetStringByteImage(pathToImage);
            byte[] byteImage = Cryptograchia.ToAes256(stringImage);
            string stringImageCrypt = BitConverter.ToString(byteImage);
            int[] delimeterToPart = WorkWirhImages.RandomDelimeter(stringImageCrypt, countPart);
            string[] folderToSave = WorkWithFolder.DistributeFileToFolder(countPart);
            ReadAndSaveFile.SaveFile(delimeterToPart, stringImageCrypt, folderToSave);
        }

        static public void StartSeparate(string path)
        {
            StartProgram.SeparateImage(path);
        }

        static public void AssemblyImage(string pathToMain)
        {
            if (!File.Exists(pathToMain))
                throw new FileNotFoundException();
            if (new FileInfo(pathToMain).Length == 0)
                throw new Exception("File is empty!");

            string[] textFromFile = File.ReadAllLines(pathToMain);
            string stringImageCrypt = ReadAndSaveFile.ReadFile(pathToMain);


            int ocunt = stringImageCrypt.Length;
            var encoding = new System.Text.UTF8Encoding();
            string[] tempAry = stringImageCrypt.Split('-');
            byte[] byteImage = new byte[tempAry.Length];
            for (int i = 0; i < tempAry.Length; i++)
                byteImage[i] = Convert.ToByte(tempAry[i], 16);

            string stringImage = Cryptograchia.FromAes256(byteImage);
            
            WorkWirhImages.GetImageFromByte(stringImage);
        }
    }

    static class ReadAndSaveFile
    {
        private static string mainFile = @"/storage/sdcard0/Main.txt";

        private static string mainFolder = @"/storage/sdcard0";
        //сохранение изо по папкам, передаем массив где i-номер части, x[i]-путь
        static private void CreateSaveFile(string[] folderToSaveAllImage)
        {
            string outString = "";
            for(int i=0;i<folderToSaveAllImage.Count();i++)
            {
                outString += folderToSaveAllImage[i] + "\n";
            }
            File.WriteAllText(mainFile, outString);
        }
        
        static public void SaveFile(int[] randomCount, string cryptImage, string[] folderToSave)
        {
            int countPart = folderToSave.Count();
            string text = "";
            string nameFile = "";
            string command, localPath="";
            string[] splitPath;

            for (int i =0; i<countPart; i++)
            {
                text = cryptImage.Substring(0, randomCount[i]);
                cryptImage = cryptImage.Remove(0, randomCount[i]);

                nameFile = mainFolder + "/part" + i + ".txt";
                File.WriteAllText(nameFile, text);
                splitPath = folderToSave[i].Split(new char[] { '/' });
                localPath = "";
                for (int j = 1; j < splitPath.Length-1; j++)
                    localPath = localPath + '/' + splitPath[j];

                command = "cp " + nameFile + " " + folderToSave[i];
                Java.Lang.Runtime.GetRuntime().Exec(new string[] { "su", "-c", command});
            }
            CreateSaveFile(folderToSave);
        }

        static public string ReadFile(string pathToMainFile)
        {
            string[] lineFromFile = File.ReadAllLines(pathToMainFile);
            File.Delete(pathToMainFile);
            string text = "";
            foreach(string line in lineFromFile)
            {
                text += File.ReadAllText(line);
                File.Delete(line);
            }
            return text;
        }
    }
    

    static class WorkWithFolder
    {
        static private string[] folderToSave = new string[]
        {          
            //@"/system/bin",
            //@"/system/xbin"

            @"/storage/sdcard0"
        };

        static public string[] DistributeFileToFolder(int countDelimeters = 3)
        {
            Random rnd = new Random();
            string[] resultDistrib = new string[countDelimeters];
            
            for (int i = 0; i < countDelimeters; i++)
            {

                resultDistrib[i] = folderToSave[rnd.Next(folderToSave.Count())]+ @"/part" + i + ".txt";

            }
            return resultDistrib;
        }
    }

    static class WorkWirhImages
    {
        static public string GetStringByteImage(string path)
        {
            byte[] pic = File.ReadAllBytes(path);
            string stringImage = Convert.ToBase64String(pic);
            return stringImage;
        }

        static public void GetImageFromByte(string stringImage)
        {
            byte[] byteImage = Convert.FromBase64String(stringImage);
            string filepath = @"/storage/sdcard0/out.jpg";
            File.WriteAllBytes(filepath, byteImage);
        }

        static public int[] RandomDelimeter(string stringImage, int countDelimeters=3)
        {
            
            int countSymbol = stringImage.Length;
            Random rnd = new Random();
            int[] randomCount = new int[countDelimeters];
            for (int i=0; i<countDelimeters; i++)
            {
                //На сколько символов делить
                if (i==countDelimeters-1)
                {
                    randomCount[i] = countSymbol;
                }
                else 
                { 
                    //Рандом специально, чтобы минимум один символ остался на последний файл
                    randomCount[i] = rnd.Next(1, countSymbol-1);
                    //Сколько символов осталось при делении
                    countSymbol -= randomCount[i];
                }
                //Console.WriteLine(randomCount[i]);
            }
            return randomCount;
        }
    }

    static class Cryptograchia
    {
        static private byte[] aeskey = new byte[] { 30, 69, 137, 76, 95, 187, 114, 36, 149, 203, 101, 170, 179, 5, 77, 220, 179, 236, 66, 174, 9, 45, 120, 155, 99, 42, 53, 192, 195, 12, 237, 253 };
        static public byte[] ToAes256(string src)
        {
            //Объявляем объект класса AES
            Aes aes = Aes.Create();
            //Генерируем соль
            aes.GenerateIV();
            //Присваиваем ключ. aeskey - переменная (массив байт), сгенерированная методом GenerateKey() класса AES
            aes.Key = aeskey;
            byte[] encrypted;
            ICryptoTransform crypt = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(src);
                    }
                }
                //Записываем в переменную encrypted зашиврованный поток байтов
                encrypted = ms.ToArray();
            }
            //Возвращаем поток байт + крепим соль
            return encrypted.Concat(aes.IV).ToArray();
        }

        static public string FromAes256(byte[] shifr)
        {
            byte[] bytesIv = new byte[16];
            byte[] mess = new byte[shifr.Length - 16];
            //Списываем соль
            for (int i = shifr.Length - 16, j = 0; i < shifr.Length; i++, j++)
                bytesIv[j] = shifr[i];
            //Списываем оставшуюся часть сообщения
            for (int i = 0; i < shifr.Length - 16; i++)
                mess[i] = shifr[i];
            //Объект класса Aes
            Aes aes = Aes.Create();
            //Задаем тот же ключ, что и для шифрования
            aes.Key = aeskey;
            //Задаем соль
            aes.IV = bytesIv;
            //Строковая переменная для результата
            string text = "";
            byte[] data = mess;
            ICryptoTransform crypt = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (CryptoStream cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        //Результат записываем в переменную text в вие исходной строки
                        text = sr.ReadToEnd();
                    }
                }
            }
            return text;
        }
    }
    
}
