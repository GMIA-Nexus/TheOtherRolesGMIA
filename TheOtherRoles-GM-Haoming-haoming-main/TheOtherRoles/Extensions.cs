using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// Token: 0x0200005D RID: 93
namespace TheOtherRoles
{
    public static class Extensions
    {
        // Token: 0x060002DF RID: 735 RVA: 0x00012E79 File Offset: 0x00011079
        public static IEnumerator WaitForCompletion(this Task self)
        {
            while (!self.IsCompleted)
            {
                yield return null;
            }
            yield break;
        }

        // Token: 0x060002E0 RID: 736 RVA: 0x00012E88 File Offset: 0x00011088
        public static string RemoveAll(this string self, params char[] chars)
        {
            StringBuilder stringBuilder = new(self.Length);
            foreach (char value in self)
            {
                if (!chars.Contains(value))
                {
                    stringBuilder.Append(value);
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x060002E1 RID: 737 RVA: 0x00012ED4 File Offset: 0x000110D4
        public static void TrimEnd(this StringBuilder self)
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                char c = self[i];
                if (c is not ' ' and not '\t' and not '\n' and not '\r')
                {
                    break;
                }
                int length = self.Length;
                self.Length = length - 1;
            }
        }

        // Token: 0x060002E2 RID: 738 RVA: 0x00012F20 File Offset: 0x00011120
        public static void DestroyAll<T>(this IList<T> self) where T : MonoBehaviour
        {
            for (int i = 0; i < self.Count; i++)
            {
                UnityEngine.Object.Destroy(self[i].gameObject);
            }
            self.Clear();
        }

        // Token: 0x060002E3 RID: 739 RVA: 0x00012F5A File Offset: 0x0001115A
        public static void AddUnique<T>(this IList<T> self, T item)
        {
            if (!self.Contains(item))
            {
                self.Add(item);
            }
        }

        // Token: 0x060002E4 RID: 740 RVA: 0x00012F6C File Offset: 0x0001116C
        public static string ToTextColor(this Color c)
        {
            return string.Concat(new string[]
            {
                "<color=#",
                Extensions.ByteHex[(int)(byte)(c.r * 255f)],
                Extensions.ByteHex[(int)(byte)(c.g * 255f)],
                Extensions.ByteHex[(int)(byte)(c.b * 255f)],
                Extensions.ByteHex[(int)(byte)(c.a * 255f)],
                ">"
            });
        }

        // Token: 0x060002E5 RID: 741 RVA: 0x00012FEC File Offset: 0x000111EC
        public static Color SetAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        // Token: 0x060002E6 RID: 742 RVA: 0x00013008 File Offset: 0x00011208
        public static int ToInteger(this Color c, bool alpha)
        {
            if (alpha)
            {
                return (int)(byte)(c.r * 256f) << 24 | (int)(byte)(c.g * 256f) << 16 | (int)(byte)(c.b * 256f) << 8 | (int)(byte)(c.a * 256f);
            }
            return (int)(byte)(c.r * 256f) << 16 | (int)(byte)(c.g * 256f) << 8 | (int)(byte)(c.b * 256f);
        }

        // Token: 0x060002E7 RID: 743 RVA: 0x00013086 File Offset: 0x00011286
        public static bool HasAnyBit(this int self, int bit)
        {
            return (self & bit) != 0;
        }

        // Token: 0x060002E8 RID: 744 RVA: 0x0001308E File Offset: 0x0001128E
        public static bool HasAnyBit(this byte self, byte bit)
        {
            return (self & bit) > 0;
        }

        // Token: 0x060002E9 RID: 745 RVA: 0x00013096 File Offset: 0x00011296
        public static bool HasAnyBit(this ushort self, byte bit)
        {
            return (self & (ushort)bit) > 0;
        }

        // Token: 0x060002EA RID: 746 RVA: 0x0001309E File Offset: 0x0001129E
        public static bool HasBit(this byte self, byte bit)
        {
            return (self & bit) == bit;
        }

        // Token: 0x060002EB RID: 747 RVA: 0x000130A8 File Offset: 0x000112A8
        public static int BitCount(this byte self)
        {
            int num = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((1 << i & (int)self) != 0)
                {
                    num++;
                }
            }
            return num;
        }

        // Token: 0x060002EC RID: 748 RVA: 0x000130D4 File Offset: 0x000112D4
        public static int IndexOf<T>(this T[] self, T item) where T : class
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        // Token: 0x060002ED RID: 749 RVA: 0x00013108 File Offset: 0x00011308
        public static int IndexOfMin<T>(this T[] self, Func<T, float> comparer)
        {
            float num = float.MaxValue;
            int result = -1;
            for (int i = 0; i < self.Length; i++)
            {
                float num2 = comparer(self[i]);
                if (num2 <= num)
                {
                    result = i;
                    num = num2;
                }
            }
            return result;
        }

        // Token: 0x060002EE RID: 750 RVA: 0x00013144 File Offset: 0x00011344

        public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            KeyValuePair<byte, int> result = new(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }

        // Token: 0x060002EF RID: 751 RVA: 0x000131CC File Offset: 0x000113CC
        public static TV GetValueOrSetDefault<TK, TV>(this Dictionary<TK, TV> self, TK key, Func<TV> defaultValueFunc)
        {
            if (!self.TryGetValue(key, out TV tv))
            {
                tv = defaultValueFunc();
                self[key] = tv;
            }
            return tv;
        }

        // Token: 0x060002F0 RID: 752 RVA: 0x000131F4 File Offset: 0x000113F4
        public static void SetAll<T>(this IList<T> self, T value)
        {
            for (int i = 0; i < self.Count; i++)
            {
                self[i] = value;
            }
        }

        // Token: 0x060002F1 RID: 753 RVA: 0x0001321C File Offset: 0x0001141C
        public static void AddAll<T>(this List<T> self, IList<T> other)
        {
            int num = self.Count + other.Count;
            if (self.Capacity < num)
            {
                self.Capacity = num;
            }
            for (int i = 0; i < other.Count; i++)
            {
                self.Add(other[i]);
            }
        }

        // Token: 0x060002F2 RID: 754 RVA: 0x00013268 File Offset: 0x00011468
        public static void RemoveDupes<T>(this IList<T> self) where T : class
        {
            for (int i = 0; i < self.Count; i++)
            {
                T t = self[i];
                for (int j = self.Count - 1; j > i; j--)
                {
                    if (self[j] == t)
                    {
                        self.RemoveAt(j);
                    }
                }
            }
        }

        // Token: 0x060002F3 RID: 755 RVA: 0x000132BC File Offset: 0x000114BC
        public static void Shuffle<T>(this IList<T> self, int startAt = 0)
        {
            for (int i = startAt; i < self.Count - 1; i++)
            {
                T value = self[i];
                int index = UnityEngine.Random.Range(i, self.Count);
                self[i] = self[index];
                self[index] = value;
            }
        }

        // Token: 0x060002F4 RID: 756 RVA: 0x00013308 File Offset: 0x00011508
        public static void Shuffle<T>(this System.Random r, IList<T> self)
        {
            for (int i = 0; i < self.Count; i++)
            {
                T value = self[i];
                int index = r.Next(self.Count);
                self[i] = self[index];
                self[index] = value;
            }
        }

        // Token: 0x060002F5 RID: 757 RVA: 0x00013354 File Offset: 0x00011554
        public static T[] RandomSet<T>(this IList<T> self, int length)
        {
            T[] array = new T[length];
            self.RandomFill(array);
            return array;
        }

        // Token: 0x060002F6 RID: 758 RVA: 0x00013370 File Offset: 0x00011570
        public static void RandomFill<T>(this IList<T> self, T[] target)
        {
            HashSet<int> hashSet = new();
            for (int i = 0; i < target.Length; i++)
            {
                int num;
                do
                {
                    num = self.RandomIdx<T>();
                }
                while (hashSet.Contains(num));
                target[i] = self[num];
                hashSet.Add(num);
                if (hashSet.Count == self.Count)
                {
                    return;
                }
            }
        }

        // Token: 0x060002F7 RID: 759 RVA: 0x000133C6 File Offset: 0x000115C6
        public static int RandomIdx<T>(this IList<T> self)
        {
            return UnityEngine.Random.Range(0, self.Count);
        }

        // Token: 0x060002F8 RID: 760 RVA: 0x000133D4 File Offset: 0x000115D4
        public static int RandomIdx<T>(this IEnumerable<T> self)
        {
            return UnityEngine.Random.Range(0, self.Count<T>());
        }

        // Token: 0x060002F9 RID: 761 RVA: 0x000133E2 File Offset: 0x000115E2
        public static T Random<T>(this IEnumerable<T> self)
        {
            return self.ToArray<T>().Random<T>();
        }

        // Token: 0x060002FA RID: 762 RVA: 0x000133F0 File Offset: 0x000115F0
        public static T Random<T>(this IList<T> self)
        {
            if (self.Count > 0)
            {
                return self[UnityEngine.Random.Range(0, self.Count)];
            }
            return default;
        }

        // Token: 0x060002FB RID: 763 RVA: 0x00013422 File Offset: 0x00011622
        public static Vector2 Div(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        // Token: 0x060002FC RID: 764 RVA: 0x00013443 File Offset: 0x00011643
        public static Vector2 Mul(this Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        // Token: 0x060002FD RID: 765 RVA: 0x00013464 File Offset: 0x00011664
        public static Vector3 Mul(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        // Token: 0x060002FE RID: 766 RVA: 0x00013492 File Offset: 0x00011692
        public static Vector3 Inv(this Vector3 a)
        {
            return new Vector3(1f / a.x, 1f / a.y, 1f / a.z);
        }

        // Token: 0x060002FF RID: 767 RVA: 0x000134C0 File Offset: 0x000116C0
        public static Rect Lerp(this Rect source, Rect target, float t)
        {
            Rect result = default;
            result.position = Vector2.Lerp(source.position, target.position, t);
            result.size = Vector2.Lerp(source.size, target.size, t);
            return result;
        }

        // Token: 0x06000300 RID: 768 RVA: 0x0001350C File Offset: 0x0001170C
        public static void ForEach<T>(this IList<T> self, Action<T> todo)
        {
            for (int i = 0; i < self.Count; i++)
            {
                todo(self[i]);
            }
        }

        // Token: 0x06000301 RID: 769 RVA: 0x00013538 File Offset: 0x00011738
        public static T Max<T>(this IList<T> self, Func<T, float> comparer)
        {
            T t = self.First<T>();
            float num = comparer(t);
            for (int i = 0; i < self.Count; i++)
            {
                T t2 = self[i];
                float num2 = comparer(t2);
                if (num < num2 || (num == num2 && UnityEngine.Random.value > 0.5f))
                {
                    num = num2;
                    t = t2;
                }
            }
            return t;
        }

        // Token: 0x06000302 RID: 770 RVA: 0x00013594 File Offset: 0x00011794
        public static T Max<T>(this IList<T> self, Func<T, decimal> comparer)
        {
            T t = self.First<T>();
            decimal d = comparer(t);
            for (int i = 0; i < self.Count; i++)
            {
                T t2 = self[i];
                decimal num = comparer(t2);
                if (d < num || (d == num && UnityEngine.Random.value > 0.5f))
                {
                    d = num;
                    t = t2;
                }
            }
            return t;
        }

        // Token: 0x06000303 RID: 771 RVA: 0x000135F8 File Offset: 0x000117F8
        public static int Wrap(this int self, int max)
        {
            if (self >= 0)
            {
                return self % max;
            }
            return (self + -(self / max) * max + max) % max;
        }

        // Token: 0x06000304 RID: 772 RVA: 0x00013610 File Offset: 0x00011810
        public static int LastIndexOf<T>(this T[] self, Predicate<T> pred)
        {
            for (int i = self.Length - 1; i > -1; i--)
            {
                if (pred(self[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        // Token: 0x06000305 RID: 773 RVA: 0x00013640 File Offset: 0x00011840
        public static int IndexOf<T>(this T[] self, Predicate<T> pred)
        {
            for (int i = 0; i < self.Length; i++)
            {
                if (pred(self[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        // Token: 0x06000306 RID: 774 RVA: 0x00013670 File Offset: 0x00011870
        public static Vector2 MapToRectangle(this Vector2 del, Vector2 widthAndHeight)
        {
            del = del.normalized;
            if (Mathf.Abs(del.x) > Mathf.Abs(del.y))
            {
                return new Vector2(Mathf.Sign(del.x) * widthAndHeight.x, del.y * widthAndHeight.y / 0.70710677f);
            }
            return new Vector2(del.x * widthAndHeight.x / 0.70710677f, Mathf.Sign(del.y) * widthAndHeight.y);
        }

        // Token: 0x06000307 RID: 775 RVA: 0x000136F3 File Offset: 0x000118F3
        public static float AngleSignedRad(this Vector2 vector1, Vector2 vector2)
        {
            return Mathf.Atan2(vector2.y, vector2.x) - Mathf.Atan2(vector1.y, vector1.x);
        }

        // Token: 0x06000308 RID: 776 RVA: 0x00013718 File Offset: 0x00011918
        public static float AngleSigned(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.AngleSignedRad(vector2) * 57.29578f;
        }

        // Token: 0x06000309 RID: 777 RVA: 0x00013727 File Offset: 0x00011927
        public static float AngleSigned(this Vector2 vector1)
        {
            return Mathf.Atan2(vector1.y, vector1.x);
        }

        // Token: 0x0600030A RID: 778 RVA: 0x0001373C File Offset: 0x0001193C
        public static float WheelAngle(this Vector2 vector1, Vector2 vector2)
        {
            float num = vector1.AngleSignedRad(vector2) * 57.29578f;
            if (num > 180f)
            {
                num -= 360f;
            }
            if (num < -180f)
            {
                num += 360f;
            }
            return num;
        }

        // Token: 0x0600030B RID: 779 RVA: 0x00013778 File Offset: 0x00011978
        public static Vector2 Rotate(this Vector2 self, float degrees)
        {
            float num = 0.017453292f * degrees;
            float num2 = Mathf.Cos(num);
            float num3 = Mathf.Sin(num);
            return new Vector2(self.x * num2 - num3 * self.y, self.x * num3 + num2 * self.y);
        }

        // Token: 0x0600030C RID: 780 RVA: 0x000137C0 File Offset: 0x000119C0
        public static Vector3 RotateZ(this Vector3 self, float degrees)
        {
            float num = 0.017453292f * degrees;
            float num2 = Mathf.Cos(num);
            float num3 = Mathf.Sin(num);
            return new Vector3(self.x * num2 - num3 * self.y, self.x * num3 + num2 * self.y, self.z);
        }

        // Token: 0x0600030D RID: 781 RVA: 0x00013810 File Offset: 0x00011A10
        public static Vector3 RotateY(this Vector3 self, float degrees)
        {
            float num = 0.017453292f * degrees;
            float num2 = Mathf.Cos(num);
            float num3 = Mathf.Sin(num);
            return new Vector3(self.x * num2 - num3 * self.z, self.y, self.x * num3 + num2 * self.z);
        }

        // Token: 0x0600030E RID: 782 RVA: 0x0001385E File Offset: 0x00011A5E
        public static bool TryToEnum<TEnum>(this string strEnumValue, out TEnum enumValue)
        {
            enumValue = default;
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                return false;
            }
            enumValue = (TEnum)(object)Enum.Parse(typeof(TEnum), strEnumValue);
            return true;
        }

        // Token: 0x0600030F RID: 783 RVA: 0x00013898 File Offset: 0x00011A98
        public static TEnum ToEnum<TEnum>(this string strEnumValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                return default;
            }
            return (TEnum)(object)Enum.Parse(typeof(TEnum), strEnumValue);
        }

        // Token: 0x06000310 RID: 784 RVA: 0x000138D6 File Offset: 0x00011AD6
        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            {
                return defaultValue;
            }
            return (TEnum)(object)Enum.Parse(typeof(TEnum), strEnumValue);
        }

        // Token: 0x06000311 RID: 785 RVA: 0x00013901 File Offset: 0x00011B01
        public static bool IsNullOrWhiteSpace(this string s)
        {
            if (s == null)
            {
                return true;
            }
            return !s.Any((char c) => !char.IsWhiteSpace(c));
        }

        // Token: 0x04000383 RID: 899
        private static string[] ByteHex = (from x in Enumerable.Range(0, 256)
                                           select x.ToString("X2")).ToArray<string>();
    }
}
