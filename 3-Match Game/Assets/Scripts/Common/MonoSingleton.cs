using UnityEngine;

// MonoBehaviour를 상속받는 모노싱글톤 클래스입니다.
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 유일한 인스턴스를 저장할 정적 변수입니다.
    private static T _instance;

    // 유일한 인스턴스에 접근할 수 있는 프로퍼티입니다.
    public static T Instance
    {
        get
        {
            // 인스턴스가 없는 경우
            if (_instance == null)
            {
                // 씬에서 해당 타입의 인스턴스를 찾습니다.
                _instance = FindObjectOfType<T>();

                // 씬에서 해당 타입의 인스턴스를 찾지 못한 경우
                if (_instance == null)
                {
                    // 새로운 게임 오브젝트를 생성하고 해당 타입의 컴포넌트를 추가합니다.
                    _instance = new GameObject("@" + typeof(T).Name,
                                               typeof(T)).GetComponent<T>();
                    DontDestroyOnLoad(_instance);
                }
            }
            return _instance;
        }
    }

    // MonoBehaviour의 Awake 메서드를 재정의합니다.
    protected virtual void Awake()
    {
        // 인스턴스가 없는 경우
        if (_instance == null)
        {
            // 현재 인스턴스를 할당하고 씬 변경 시 파괴되지 않도록 설정합니다.
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 인스턴스가 있는 경우 중복된 인스턴스를 파괴합니다.
            Destroy(gameObject);
        }
    }
}
