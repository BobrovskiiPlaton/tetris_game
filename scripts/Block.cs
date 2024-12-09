using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public int _x;
    public int _y;
    public int _value;
    public Structure _structure;

    public TextMesh text;

    public void Construct(int x, int y, int value, Structure structure)
    {
        _x = x;
        _y = y;
        _value = value;
        _structure = structure;
        _structure.blocks.Add(this);
        text.text = value.ToString();
    }
}
