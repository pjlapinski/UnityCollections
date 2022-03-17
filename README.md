# UnityCollections

This project's purpose is to provide an alternative to System.Collections.Generic, that Unity will be able to serialize.

# Important note:
The collections are able to serialize references to GameObjects/MonoBehaviours, but they cannot serialize fields of classes, even marked as Serializable. 
This limitation does not apply to structs with primitive fields.

What this means, as an example, that:
```csharp
[Serializable]
public class B
{
  public bool a;
  public int b;
}

[Serializable]
public struct C
{
  public B c;
}

public class A : MonoBehaviour
{
  [SerializeField] private Dictionary<string, B> _dictionary1;
  [SerializeField] private Dictionary<string, C> _dictionary2;
}
```

will not be able to be serialized, and the values in the editor will keep reverting to initial values. However, as mentioned earlier, structs and GameObject/MonoBehaviour references work correctly, as shown in example below.

```csharp

[Serializable]
public struct B
{
  public bool a;
  public int b;
}

public class C : MonoBehaviour {}

public enum D { a, b }

public class A : MonoBehaviour
{
  [SerializeField] private Dictionary<string, B> _dictionary1;
  [SerializeField] private Dictionary<string, C> _dictionary2;
  [SerializeField] private Dictionary<string, int> _dictionary3;
  [SerializeField] private Dictionary<string, D> _dictionary4;
}
```

