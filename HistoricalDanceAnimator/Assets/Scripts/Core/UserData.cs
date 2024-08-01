using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    private static UserData s_instance;

    public static UserData GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new UserData();
        }

        return s_instance;
    }

    public string danceName;

    public UserData()
    {
        danceName = "";
    }
}
