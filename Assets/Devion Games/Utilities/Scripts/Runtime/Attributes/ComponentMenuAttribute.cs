using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ComponentMenu : Attribute
{
    private string m_ComponentMenu;

    public string componentMenu
    {
        get
        {
            return this.m_ComponentMenu;
        }
    }

    public ComponentMenu(string menuName)
    {
        this.m_ComponentMenu = menuName;
    }
}