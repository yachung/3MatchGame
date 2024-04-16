using UnityEngine;

// MonoBehaviour�� ��ӹ޴� ���̱��� Ŭ�����Դϴ�.
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // ������ �ν��Ͻ��� ������ ���� �����Դϴ�.
    private static T _instance;

    // ������ �ν��Ͻ��� ������ �� �ִ� ������Ƽ�Դϴ�.
    public static T Instance
    {
        get
        {
            // �ν��Ͻ��� ���� ���
            if (_instance == null)
            {
                // ������ �ش� Ÿ���� �ν��Ͻ��� ã���ϴ�.
                _instance = FindObjectOfType<T>();

                // ������ �ش� Ÿ���� �ν��Ͻ��� ã�� ���� ���
                if (_instance == null)
                {
                    // ���ο� ���� ������Ʈ�� �����ϰ� �ش� Ÿ���� ������Ʈ�� �߰��մϴ�.
                    _instance = new GameObject("@" + typeof(T).Name,
                                               typeof(T)).GetComponent<T>();
                    DontDestroyOnLoad(_instance);
                }
            }
            return _instance;
        }
    }

    // MonoBehaviour�� Awake �޼��带 �������մϴ�.
    protected virtual void Awake()
    {
        // �ν��Ͻ��� ���� ���
        if (_instance == null)
        {
            // ���� �ν��Ͻ��� �Ҵ��ϰ� �� ���� �� �ı����� �ʵ��� �����մϴ�.
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // �̹� �ν��Ͻ��� �ִ� ��� �ߺ��� �ν��Ͻ��� �ı��մϴ�.
            Destroy(gameObject);
        }
    }
}
