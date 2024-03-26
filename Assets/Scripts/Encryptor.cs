using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class Encryptor
{
    public static string SHA256Encryt(string plainText)
    {
        SHA256 encrpytSHA256 = SHA256.Create();
        byte[] hashValue = encrpytSHA256.ComputeHash(Encoding.Default.GetBytes(plainText));
        string resultSHA256 = Convert.ToBase64String(hashValue);
        return resultSHA256;
    }
}
