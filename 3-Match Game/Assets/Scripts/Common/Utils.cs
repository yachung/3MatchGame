using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Utils
{
    #region Random
    /// <summary>
    /// 임의의 확률을넣으면 성공인지 실패인지 bool값 리턴 
    /// </summary>
    /// <param name="percent"></param>
    /// <returns></returns>
    public static bool GetSimpleRandomResult(uint percent = 50)
    {
        int simplePercent = (int)percent;
        return simplePercent >= UnityEngine.Random.Range(0, 100);
    }

    public static T FindRandom<T>(this List<T> list)
    {
        if (list.Count > 0)
            return list[UnityEngine.Random.Range(0, list.Count - 1)];
        else
            return default;
    }
    #endregion

    #region Color
    public static Color GetCalcColor(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static Color GetColorByHex(string hex)
    {
        if (hex.First() != '#')
            hex = string.Format("#{0}", hex);

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;
        else
            return Color.white;
    }
    #endregion

    #region Extension Methods
    public static int ToInt(this Enum enumValue)
    {
        return Convert.ToInt32(enumValue);
    }

    public static int ToCode(this Enum enumValue)
    {
        var da = (DescriptionAttribute[])(enumValue.GetType().GetField(enumValue.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
        string destValue = da.Length > 0 ? da[0].Description : "0";
        return Int32.Parse(destValue);
    }

    /// <summary>
    /// 해당 enum의 Description값을 string으로 반환
    /// </summary>
    public static string GetDescription(this Enum _enum)
    {
        Type type = _enum.GetType();
        MemberInfo[] memInfo = type.GetMember(_enum.ToString());

        if (memInfo != null && memInfo.Length > 0)
        {
            //해당 text 값을 배열로 추출
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        // 만약 추출실패할경우 그냥 enum값 string으로 반환
        return _enum.ToString();
    }

    #endregion

    #region Mathf
    public static int CeilToInt(float variable)
    {
        int result = 0;

        if (variable % 1 >= 0.001f)
            result = (Mathf.CeilToInt(variable));
        else
            result = (int)variable;

        return result;
    }

    public static Vector2 GetVector2BezierCurve(Vector2 v1, Vector2 v2, Vector2 v3, float fStep)
    {
        if (fStep >= 1f)
            fStep = 1f;

        if (fStep <= 0f)
            fStep = 0f;

        Vector2 result;

        float fStep2 = fStep * fStep;
        float fStepInv = 1.0f - fStep;
        float fStepInv2 = fStepInv * fStepInv;

        result.x = v1.x * fStepInv2 + 2.0f * v2.x * fStepInv * fStep + v3.x * fStep2;
        result.y = v1.y * fStepInv2 + 1.0f * v2.y * (fStepInv * fStep * 0.5f) + v3.y * fStep2;

        return result;
    }
    #endregion
}
