using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles.MetaContext
{
    [Flags]
    public enum GUIAlignment
    {
        /// <summary>
        /// 中央に配置します。
        /// </summary>
        Center = 0b0000,
        /// <summary>
        /// 左側に寄せて配置します。
        /// </summary>
        Left = 0b0001,
        /// <summary>
        /// 右側に寄せて配置します。
        /// </summary>
        Right = 0b0010,
        /// <summary>
        /// 下方に寄せて配置します。
        /// </summary>
        Bottom = 0b0100,
        /// <summary>
        /// 上方に寄せて配置します。
        /// </summary>
        Top = 0b1000,

        /// <summary>
        /// 上方左側に寄せて配置します。
        /// </summary>
        TopLeft = Left | Top,
        /// <summary>
        /// 上方右側に寄せて配置します。
        /// </summary>
        TopRight = Right | Top,
        /// <summary>
        /// 下方左側に寄せて配置します。
        /// </summary>
        BottomLeft = Left | Bottom,
        /// <summary>
        /// 下方右側に寄せて配置します。
        /// </summary>
        BottomRight = Right | Bottom,
    }

    /// <summary>
    /// 画面の基準点を表します。
    /// 生成されたスクリーン上の点と空間上の点は同じ位置にくるように重ね合わせられます。
    /// </summary>
    /// <param name="pivot">重ね合わせるスクリーン上の点</param>
    /// <param name="anchoredPosition">重ね合わせられる空間上の点</param>
    public record Anchor(Vector2 pivot, Vector3 anchoredPosition)
    {
        public static Anchor At(Vector2 pivot) => new(pivot, new(0f, 0f, 0f));
    }

    /// <summary>
    /// GUIの画面を表します。
    /// </summary>
    public interface GUIScreen
    {
        /// <summary>
        /// スクリーン上の表示を更新します。
        /// </summary>
        /// <param name="context">表示するコンテキスト定義</param>
        /// <param name="actualSize">生成された画面の大きさ</param>
        void SetContext(GUIContext context, out Size actualSize);
    }

    public interface Image : IManageableAsset
    {
        internal Sprite GetSprite();
    }

    public interface MultiImage : IManageableAsset
    {
        internal Sprite GetSprite(int index);
        Image AsLoader(int index);
    }

    public interface IManageableAsset
    {
        void UnloadAsset();
        IEnumerator LoadAsset();
        void MarkAsUnloadAsset();
    }

    public interface Artifact<T> : IEnumerable<T>
    {
        void Do(Action<T> action);
    }

    internal class ListArtifact<T> : Artifact<T>
    {
        internal List<T> Values { get; init; } = new();

        public ListArtifact()
        {

        }

        public void Do(Action<T> action)
        {
            foreach (var t in Values) action.Invoke(t);
        }

        public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    }

    internal class GeneralizedArtifact<T, V> : Artifact<T> where V : T where T : class
    {
        internal Artifact<V> Inner { get; init; }

        public GeneralizedArtifact(Artifact<V> inner)
        {
            Inner = inner;
        }

        public void Do(Action<T> action) => Inner.Do(v => action(v));

        public IEnumerator<T> GetEnumerator() => Inner.Select(v => v as T).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Inner.Select(v => v as T).GetEnumerator();
    }

    public delegate GUIContext GUIContextSupplier();
    public delegate void GUIClickableAction(GUIClickable clickable);

    /// <summary>
    /// GUI上に表示できるオブジェクトの定義を表します。
    /// </summary>
    public abstract class GUIContext
    {
        internal abstract GUIAlignment Alignment { get; }
        internal Image BackImage { get; set; } = null!;
        internal bool GrayoutedBackImage { get; set; } = false;
        internal abstract GameObject Instantiate(Size size, out Size actualSize);
        internal abstract GameObject Instantiate(Anchor anchor, Size size, out Size actualSize);

        public static implicit operator GUIContextSupplier(GUIContext context) => () => context ?? TORGUIContextEngine.Instance.EmptyContext;
    }

    /// <summary>
    /// GUI上のクリック可能なオブジェクトを表します。
    /// </summary>
    public class GUIClickable
    {
        internal PassiveUiElement uiElement { get; init; }
        public IGUISelectable Selectable { get; private init; }

        internal GUIClickable(PassiveUiElement uiElement, IGUISelectable selectable = null)
        {
            this.uiElement = uiElement;
            this.Selectable = selectable;
        }
    }

    public interface IGUISelectable
    {
        void Unselect();
        void Select();
        public bool IsSelected { get; }
    }

    /// <summary>
    /// GUI上に表示する各種オブジェクトの定義や関連するオブジェクトを生成できます。
    /// </summary>
    public interface GUI
    {
        /// <summary>
        /// 何も表示しないウィジェットです。
        /// </summary>
        GUIContext EmptyContext { get; }

        /// <summary>
        /// 生文字列のコンテキストです。
        /// Textメソッドの呼び出しを簡素化した冗長なメソッドです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="rawText">表示する文字列</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext RawText(GUIAlignment alignment, TextAttributes attribute, string rawText);

        /// <summary>
        /// 翻訳キーに対応する文字列のコンテキストです。
        /// Textメソッドの呼び出しを簡素化した冗長なメソッドです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="translationKey">翻訳キー</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext LocalizedText(GUIAlignment alignment, TextAttributes attribute, string translationKey);

        /// <summary>
        /// 文字列のコンテキストです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="text">テキストを表すコンポーネント</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext Text(GUIAlignment alignment, TextAttributes attribute, TextComponent text);

        GUIContext RealtimeText(GUIAlignment alignment, TextAttributes attribute, Func<string> textSupplier, int length);

        /// <summary>
        /// 翻訳テキストを表示するボタンです。
        /// Buttonメソッドの呼び出しを簡素化した冗長なメソッドです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="translationKey">翻訳キー</param>
        /// <param name="onClick">クリックされた際に実行するアクション</param>
        /// <param name="onMouseOver">カーソルが触れた際に実行するアクション</param>
        /// <param name="onMouseOut">カーソルが離れた際に実行するアクション</param>
        /// <param name="onRightClick">右クリックされた際に実行するアクション</param>
        /// <param name="color">ボタンの色</param>
        /// <param name="selectedColor">カーソルが重なっている時のボタンの色</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext LocalizedButton(GUIAlignment alignment, TextAttributes attribute, string translationKey, Action onClick, Action onMouseOver = null, Action onMouseOut = null, Action onRightClick = null, Color? color = null, Color? selectedColor = null);

        /// <summary>
        /// 生文字列を表示するボタンです。
        /// Buttonメソッドの呼び出しを簡素化した冗長なメソッドです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="rawText">表示するテキスト</param>
        /// <param name="onClick">クリックされた際に実行するアクション</param>
        /// <param name="onMouseOver">カーソルが触れた際に実行するアクション</param>
        /// <param name="onMouseOut">カーソルが離れた際に実行するアクション</param>
        /// <param name="onRightClick">右クリックされた際に実行するアクション</param>
        /// <param name="color">ボタンの色</param>
        /// <param name="selectedColor">カーソルが重なっている時のボタンの色</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext RawButton(GUIAlignment alignment, TextAttributes attribute, string rawText, Action onClick, Action onMouseOver = null, Action onMouseOut = null, Action onRightClick = null, Color? color = null, Color? selectedColor = null);

        /// <summary>
        /// テキストを表示するボタンです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="attribute">テキストの属性</param>
        /// <param name="text">テキストを表すコンポーネント</param>
        /// <param name="onClick">クリックされた際に実行するアクション</param>
        /// <param name="onMouseOver">カーソルが触れた際に実行するアクション</param>
        /// <param name="onMouseOut">カーソルが離れた際に実行するアクション</param>
        /// <param name="onRightClick">右クリックされた際に実行するアクション</param>
        /// <param name="color">ボタンの色</param>
        /// <param name="selectedColor">カーソルが重なっている時のボタンの色</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext Button(GUIAlignment alignment, TextAttributes attribute, TextComponent text, Action onClick, Action onMouseOver = null, Action onMouseOut = null, Action onRightClick = null, Color? color = null, Color? selectedColor = null);

        /// <summary>
        /// 画像を表示するコンテキストです。
        /// </summary>
        /// <param name="alignment">画像の表示位置</param>
        /// <param name="image">画像</param>
        /// <param name="size">表示する大きさ</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext Image(GUIAlignment alignment, Image image, FuzzySize size);

        /// <summary>
        /// スクロールビューです。
        /// </summary>
        /// <param name="alignment">ビューの配置位置</param>
        /// <param name="size">ビューの大きさ</param>
        /// <param name="scrollerTag">スクローラー位置を再現するためのタグ</param>
        /// <param name="inner">ビュー内で表示するコンテキスト</param>
        /// <param name="artifact">ビュー内に生成されるスクリーンへのアクセサ</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext ScrollView(GUIAlignment alignment, Size size, string scrollerTag, GUIContext inner, out Artifact<GUIScreen> artifact);

        /// <summary>
        /// 縦方向にコンテキストを並べます。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="innerReference">並べるコンテキスト　GUIScreen.SetContextの呼び出し時に評価されます</param>
        /// <param name="fixedWidth">コンテキストの固定幅 nullの場合はフレキシブルに幅を設定します</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext VerticalHolder(GUIAlignment alignment, IEnumerable<GUIContext> innerReference, float? fixedWidth = null);

        /// <summary>
        /// 横方向にコンテキストを並べます。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="innerReference">並べるコンテキスト　GUIScreen.SetContextの呼び出し時に評価されます</param>
        /// <param name="fixedHeight">コンテキストの固定長 nullの場合はフレキシブルに高さを設定します</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext HorizontalHolder(GUIAlignment alignment, IEnumerable<GUIContext> innerReference, float? fixedHeight = null);

        /// <summary>
        /// 縦方向にコンテキストを並べます。
        /// 呼び出しを簡素化するためのオーバーロードです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="fixedWidth">コンテキストの固定幅 nullの場合はフレキシブルに幅を設定します</param>
        /// <param name="inner">並べるコンテキスト</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext VerticalHolder(GUIAlignment alignment, float fixedWidth, params GUIContext[] inner) => VerticalHolder(alignment, inner, null);

        /// <summary>
        /// 横方向にコンテキストを並べます。
        /// 呼び出しを簡素化するためのオーバーロードです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="fixedHeight">コンテキストの固定長 nullの場合はフレキシブルに高さを設定します</param>
        /// <param name="inner">並べるコンテキスト</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext HorizontalHolder(GUIAlignment alignment, float fixedHeight, params GUIContext[] inner) => HorizontalHolder(alignment, inner, null);

        /// <summary>
        /// 縦方向にコンテキストを並べます。
        /// 呼び出しを簡素化するためのオーバーロードです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="inner">並べるコンテキスト</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext VerticalHolder(GUIAlignment alignment, params GUIContext[] inner) => VerticalHolder(alignment, inner, null);

        /// <summary>
        /// 横方向にコンテキストを並べます。
        /// 呼び出しを簡素化するためのオーバーロードです。
        /// </summary>
        /// <param name="alignment">コンテキストの配置位置</param>
        /// <param name="inner">並べるコンテキスト</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext HorizontalHolder(GUIAlignment alignment, params GUIContext[] inner) => HorizontalHolder(alignment, inner, null);

        /// <summary>
        /// ウィジットを指定の個数ずつ縦方向に伸ばしながら配置します。
        /// </summary>
        /// <param name="alignment"></param>
        /// <param name="inner"></param>
        /// <param name="perLine"></param>
        /// <returns></returns>
        GUIContext Arrange(GUIAlignment alignment, IEnumerable<GUIContext> inner, int perLine);

        /// <summary>
        /// 余白を表すコンテキストです。見た目を整えるために使用します。
        /// </summary>
        /// <param name="margin">余白の大きさ</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext Margin(FuzzySize margin);

        /// <summary>
        /// 余白を表すコンテキストです。
        /// Marginの呼び出しを簡素化するための冗長なメソッドです。
        /// </summary>
        /// <param name="margin">縦方向の余白</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext VerticalMargin(float margin) => Margin(new(null, margin));

        /// <summary>
        /// 余白を表すコンテキストです。
        /// Marginの呼び出しを簡素化するための冗長なメソッドです。
        /// </summary>
        /// <param name="margin">横方向の余白</param>
        /// <returns>生成されたコンテキスト定義</returns>
        GUIContext HorizontalMargin(float margin) => Margin(new(margin, null));

        /// <summary>
        /// フォントを取得します。
        /// </summary>
        /// <param name="font">フォントの識別子</param>
        /// <returns>フォント</returns>
        Font GetFont(FontAsset font);

        /// <summary>
        /// パラメータからテキスト属性を取得します。
        /// GenerateAttributeメソッドで同等のテキスト属性を生成することもできますが、このメソッドから取得できるテキスト属性はキャッシュされており、メモリを過剰に逼迫しません。
        /// </summary>
        /// <param name="attribute">属性のパラメータ</param>
        /// <returns>テキスト属性</returns>
        TextAttributes GetAttribute(AttributeParams attribute);

        /// <summary>
        /// 識別子からテキスト属性を取得します。
        /// Nebulaで用いられているテキスト属性をそのまま借用できます。
        /// GenerateAttributeメソッドで同等のテキスト属性を生成することもできますが、このメソッドから取得できるテキスト属性はキャッシュされており、メモリを過剰に逼迫しません。
        /// </summary>
        /// <param name="attribute">属性の識別子</param>
        /// <returns>テキスト属性</returns>
        TextAttributes GetAttribute(AttributeAsset attribute);

        /// <summary>
        /// テキスト属性を生成します。
        /// 同じ属性を何度も生成すると非効率的です。テキスト属性は不変ですので、再利用できるテキスト属性は積極的に再利用を心がけてください。
        /// </summary>
        /// <param name="attribute">属性のパラメータ</param>
        /// <param name="color">テキスト色</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="size">文字列の占有する大きさの上限</param>
        /// <returns></returns>
        TextAttributes GenerateAttribute(AttributeParams attribute, Color color, FontSize fontSize, Size size);

        /// <summary>
        /// 色付き翻訳テキストコンポーネントを生成します。
        /// 呼び出しを簡素化するための冗長なメソッドです。
        /// </summary>
        /// <param name="color">テキスト色</param>
        /// <param name="transrationKey">翻訳キー</param>
        /// <returns>テキストコンポーネント</returns>
        TextComponent TextComponent(Color color, string transrationKey);

        /// <summary>
        /// 生文字列のテキストコンポーネントを生成します。
        /// </summary>
        /// <param name="rawText">表すテキスト</param>
        /// <returns>テキストコンポーネント</returns>
        TextComponent RawTextComponent(string rawText);

        /// <summary>
        /// 翻訳テキストのコンポーネントを生成します。
        /// </summary>
        /// <param name="translationKey">翻訳キー</param>
        /// <returns></returns>
        TextComponent LocalizedTextComponent(string translationKey);

        /// <summary>
        /// 色付きテキストのコンポーネントを生成します。
        /// </summary>
        /// <param name="color">テキスト色</param>
        /// <param name="component">テキストコンポーネント</param>
        /// <returns>色付きのテキストコンポーネント</returns>
        TextComponent ColorTextComponent(Color color, TextComponent component);
    }
}
