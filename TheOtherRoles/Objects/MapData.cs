using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    public abstract class MapData
    {
        abstract protected Vector2[] MapArea { get; }
        abstract public Vector3[] SpawnPos { get; }

        public bool CheckMapArea(Vector2 position, float radius = 0.1f)
        {
            if (radius > 0f)
            {
                int num = Physics2D.OverlapCircleNonAlloc(position, radius, PhysicsHelpers.colliderHits, Constants.ShipAndAllObjectsMask);
                if (num > 0) for (int i = 0; i < num; i++) if (!PhysicsHelpers.colliderHits[i].isTrigger) return false;
            }

            return CheckMapAreaInternal(position);
        }


        public bool CheckMapAreaInternal(Vector2 position)
        {
            Vector2 vector;
            float magnitude;

            foreach (Vector2 p in MapArea)
            {
                vector = p - position;
                magnitude = vector.magnitude;
                if (magnitude > 12.0f) continue;

                if (!PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized, magnitude, Constants.ShipAndAllObjectsMask)) return true;
            }

            return false;
        }

        static private MapData[] AllMapData = [new SkeldData(), new MiraData(), new PolusData(), null!, new AirshipData(), new FungleData()];
        static public MapData GetCurrentMapData() => AllMapData[GameOptionsManager.Instance.CurrentGameOptions.MapId];
    }

    public class FungleData : MapData
    {
        static private Vector2[] MapPositions =
            [ 
        //ドロップシップ
        new(-9.2f,13.4f),
        //カフェテリア
        new(-19.1f, 7.0f),new(-13.6f,5.0f),new(-20.5f,6.0f),
        //カフェ下
        new(-12.9f,2.3f),new(-21.7f,2.41f),
        //スプラッシュゾーン
        new(-20.2f,-0.3f),new(-19.8f,-2.1f),new(-16.1f,-0.1f),new(-15.6f,-1.8f),
        //キャンプファイア周辺
        new(-11.3f,2.0f),new(-0.83f,2.4f),new(-9.4f,0.2f),new(-6.9f,0.2f),
        //スプラッシュゾーン下
        new(-17.3f,-4.5f),
        //キッチン
        new(-15.4f,-9.5f),new(-17.4f,-7.5f),
        //キッチン・ジャングル間通路
        new(-11.2f,-6.1f),new(-5.5f,-14.8f),
        //ミーティング上
        new(-2.8f,2.2f),new(2.2f,1.0f),
        //ストレージ
        new(-0.6f,4.2f),new(2.3f,6.2f),new(3.3f,6.7f),
        //ミーティング・ドーム
        new(-0.15f,-1.77f),new(-4.65f,1.58f),new(-4.8f,-1.44f),
        //ラボ
        new(-7.1f,-11.9f),new(-4.5f,-6.8f),new(-3.3f,-8.9f),new(-5.4f,-10.2f),
        //ジャングル(左)
        new(-1.44f,-13.3f),new(3.8f,-12.5f),
        //ジャングル(中)
        new(7.08f,-15.3f),new(11.6f,-14.3f),
        //ジャングル(上)
        new(2.7f,-6.0f),new(12.1f,-7.3f),
        //グリーンハウス・ジャングル
        new(13.6f,-12.1f),new(6.4f,-10f),
        //ジャングル(右)
        new(15.0f,-6.7f),new(18.1f,-9.1f),
        //ジャングル(下)
        new(14.9f,-16.3f),
        //リアクター
        new(21.1f,-6.7f),
        //高台
        new(15.9f,0.4f),new(15.6f,4.3f),new(19.2f,1.78f),
        //鉱山
        new(12.5f,7.7f),new(13.4f,9.7f),
        //ルックアウト
        new(6.6f,3.8f),new(8.7f,1f),
        //梯子中間
        new(20.1f,7.2f),
        //コミュ
        new(20.9f,10.8f),new(24.1f,13.2f),new(17.9f,12.7f),
            ];

        protected override Vector2[] MapArea => MapPositions;

        public override Vector3[] SpawnPos =>
        [
            new Vector3(-10.0842f, 13.0026f, 0.013f),
            new Vector3(0.9815f, 6.7968f, 0.0068f),
            new Vector3(22.5621f, 3.2779f, 0.0033f),
            new Vector3(-1.8699f, -1.3406f, -0.0013f),
            new Vector3(12.0036f, 2.6763f, 0.0027f),
            new Vector3(21.705f, -7.8691f, -0.0079f),
            new Vector3(1.4485f, -1.6105f, -0.0016f),
            new Vector3(-4.0766f, -8.7178f, -0.0087f),
            new Vector3(2.9486f, 1.1347f, 0.0011f),
            new Vector3(-4.2181f, -8.6795f, -0.0087f),
            new Vector3(19.5553f, -12.5014f, -0.0125f),
            new Vector3(15.2497f, -16.5009f, -0.0165f),
            new Vector3(-22.7174f, -7.0523f, 0.0071f),
            new Vector3(-16.5819f, -2.1575f, 0.0022f),
            new Vector3(9.399f, -9.7127f, -0.0097f),
            new Vector3(7.3723f, 1.7373f, 0.0017f),
            new Vector3(22.0777f, -7.9315f, -0.0079f),
            new Vector3(-15.3916f, -9.3659f, -0.0094f),
            new Vector3(-16.1207f, -0.1746f, -0.0002f),
            new Vector3(-23.1353f, -7.2472f, -0.0072f),
            new Vector3(-20.0692f, -2.6245f, -0.0026f),
            new Vector3(-4.2181f, -8.6795f, -0.0087f),
            new Vector3(-9.9285f, 12.9848f, 0.013f),
            new Vector3(-8.3475f, 1.6215f, 0.0016f),
            new Vector3(-17.7614f, 6.9115f, 0.0069f),
            new Vector3(-0.5743f, -4.7235f, -0.0047f),
            new Vector3(-20.8897f, 2.7606f, 0.002f)
        ];
    }

    public class AirshipData : MapData
    {
        static private Vector2[] MapPositions =
            [ 
        //金庫
        new(-9f, 12.8f), new(-8.7f, 4.9f), new(-12.8f, 8.7f), new(-4.8f, 8.7f), new(-7.1f, 6.8f), new(-10.4f, 6.9f), new(-7f, 10.2f),
        //宿舎前
        new(-0.5f, 8.5f),
        //エンジン上
        new(-0.4f, 5f),
        //エンジン
        new(0f, -1.4f), new(3.6f, 0.1f), new(0.4f, -2.5f), new(-6.9f, 1.1f),
        //コミュ前
        new(-11f, -1f),
        //コミュ
        new(-12.3f, 0.9f),
        //コックピット
        new(-19.9f, -2.6f), new(-19.9f, 0.5f),
        //武器庫
        new(-14.5f, -3.6f), new(-9.9f, -6f), new(-15f, -9.4f),
        //キッチン
        new(-7.5f, -7.5f), new(-7f, -12.8f), new(-2.5f, -11.2f), new(-3.9f, -9.3f),
        //左展望
        new(-13.8f, -11.8f),
        //セキュ
        new(7.3f, -12.3f), new(5.8f, -10.6f),
        //右展望
        new(10.3f, -15f),
        //エレク
        new(10.5f, -8.5f),
        //エレクの9部屋
        new(10.5f, -6.3f), new(13.5f, -6.3f), new(16.5f, -6.3f), new(19.4f, -6.3f), new(13.5f, -8.8f), new(16.5f, -8.8f), new(19.4f, -8.8f), new(16.5f, -11f), new(19.4f, -11f),
        //エレク右上
        new(19.4f, -4.2f),
        //メディカル
        new(25.2f, -9.8f), new(22.9f, -6f), new(25.2f, -9.8f), new(29.5f, -6.3f),
        //貨物
        new(31.8f, -3.3f), new(34f, 1.4f), new(39f, -0.9f), new(37.6f, -3.4f), new(32.8f, 3.6f), new(35.3f, 3.6f),
        //ロミジュリ右
        new(29.8f, -1.5f),
        //ラウンジ
        new(33.7f, 7.1f), new(32.4f, 7.1f), new(30.9f, 7.1f), new(29.2f, 7.1f), new(30.8f, 5.3f), new(24.9f, 4.9f), new(27.1f, 7.3f),
        //レコード
        new(22.3f, 9.1f), new(20f, 11.5f), new(17.6f, 9.4f), new(20.1f, 6.6f),
        //ギャップ右
        new(15.4f, 9.2f), new(11.2f, 8.5f), new(12.6f, 6.2f),
        //シャワー/ロミジュリ左
        new(18.9f, 4.5f), new(17.2f, 5.2f), new(18.5f, 0f), new(21.2f, -2f), new(24f, 0.7f), new(22.3f, 2.5f),
        //メインホール
        new(10.8f, 0f), new(14.8f, 1.9f), new(11.8f, 1.8f), new(9.7f, 2.5f), new(6.2f, 2.4f), new(6.6f, -3f), new(12.7f, -2.9f),
        //ギャップ左
        new(3.8f, 8.8f),
        //ミーティング
        new(6.5f, 15.3f), new(11.8f, 14.1f), new(11.8f, 16f), new(16.3f, 15.2f),
            ];

        protected override Vector2[] MapArea => MapPositions;

        public override Vector3[] SpawnPos => [];
    }

    public class PolusData : MapData
    {
        static private Vector2[] MapPositions =
        [
        //ドロップシップ
        new(16.7f, -2.6f),
        //ドロップシップ下
        new(14.1f, -10f), new(22.0f, -7.1f),
        //エレクトリカル
        new(7.5f, -9.7f), new(3.1f, -11.7f), new(5.4f, -11.5f), new(9.6f, -12.1f),
        //O2
        new(4.7f, -19f), new(2.4f, -17f), new(3.1f, -21.7f), new(1.9f, -19.4f), new(2.4f, -23.6f), new(6.3f, -21.3f),
        //Elec,O2,Comm周辺外
        new(7.9f, -23.6f), new(9.4f, -20.1f), new(8.2f, -16.0f), new(8.0f, -14.3f), new(13.4f, -13f),
        //左上リアクター前通路
        new(10.3f, -7.4f),
        //左上リアクター
        new(4.6f, -5f),
        //Comm
        new(11.4f, -15.9f), new(11.7f, -17.3f),
        //Weapons
        new(13f, -23.5f),
        //Storage
        new(19.4f, -11.2f),
        //オフィス左下
        new(18f, -24.5f),
        //オフィス
        new(18.6f, -21.5f), new(20.2f, -19.2f), new(19.6f, -17.6f), new(19.6f, -16.4f), new(26.5f, -17.4f),
        //アドミン
        new(20f, -22.5f), new(21.4f, -25.2f), new(22.4f, -22.6f), new(25f, -20.8f),
        //デコン（左）
        new(24.1f, -24.7f),
        //スペシメン左通路
        new(27.7f, -24.7f), new(33f, -20.6f),
        //スペシメン
        new(36.8f, -21.6f), new(36.5f, -19.3f),
        //スペシメン右通路
        new(39.2f, -15.2f),
        //デコン(上)
        new(39.8f, -10f),
        //ラボ
        new(34.7f, -10.2f), new(36.4f, -8f), new(40.5f, -7.6f), new(34.5f, -6.2f), new(31.2f, -7.6f), new(28.4f, -9.6f), new(26.5f, -7f), new(26.5f, -8.3f),
        //右リアクター
        new(24.2f, -4.5f),
        //ストレージ・ラボ下・オフィス右
        new(24f, -14.6f), new(26f, -12.2f), new(29.8f, -15.7f)
        ];

        protected override Vector2[] MapArea => MapPositions;

        public override Vector3[] SpawnPos =>
        [
            new Vector3(16.6f, -1f, 0.0f), //dropship top
            new Vector3(16.6f, -5f, 0.0f), //dropship bottom
            new Vector3(20f, -9f, 0.0f), //above storrage
            new Vector3(22f, -7f, 0.0f), //right fuel
            new Vector3(25.5f, -6.9f, 0.0f), //drill
            new Vector3(29f, -9.5f, 0.0f), //lab lockers
            new Vector3(29.5f, -8f, 0.0f), //lab weather notes
            new Vector3(35f, -7.6f, 0.0f), //lab table
            new Vector3(40.4f, -8f, 0.0f), //lab scan
            new Vector3(33f, -10f, 0.0f), //lab toilet
            new Vector3(39f, -15f, 0.0f), //specimen hall top
            new Vector3(36.5f, -19.5f, 0.0f), //specimen top
            new Vector3(36.5f, -21f, 0.0f), //specimen bottom
            new Vector3(28f, -21f, 0.0f), //specimen hall bottom
            new Vector3(24f, -20.5f, 0.0f), //admin tv
            new Vector3(22f, -25f, 0.0f), //admin books
            new Vector3(16.6f, -17.5f, 0.0f), //office coffe
            new Vector3(22.5f, -16.5f, 0.0f), //office projector
            new Vector3(24f, -17f, 0.0f), //office figure
            new Vector3(27f, -16.5f, 0.0f), //office lifelines
            new Vector3(32.7f, -15.7f, 0.0f), //lavapool
            new Vector3(31.5f, -12f, 0.0f), //snowmad below lab
            new Vector3(10f, -14f, 0.0f), //below storrage
            new Vector3(21.5f, -12.5f, 0.0f), //storrage vent
            new Vector3(19f, -11f, 0.0f), //storrage toolrack
            new Vector3(10f, -12f, 0.0f), //elec fence
            new Vector3(9f, -9f, 0.0f), //elec lockers
            new Vector3(5f, -9f, 0.0f), //elec window
            new Vector3(4f, -11.2f, 0.0f), //elec tapes
            new Vector3(5.5f, -16f, 0.0f), //elec-O2 hall
            new Vector3(1f, -17.5f, 0.0f), //O2 tree hayball
            new Vector3(3f, -21f, 0.0f), //O2 middle
            new Vector3(2f, -19f, 0.0f), //O2 gas
            new Vector3(1f, -24f, 0.0f), //O2 water
            new Vector3(7f, -24f, 0.0f), //under O2
            new Vector3(9f, -20f, 0.0f), //right outside of O2
            new Vector3(7f, -15.8f, 0.0f), //snowman under elec
            new Vector3(11f, -17f, 0.0f), //comms table
            new Vector3(12.7f, -15.5f, 0.0f), //coms antenna pult
            new Vector3(13f, -24.5f, 0.0f), //weapons window
            new Vector3(15f, -17f, 0.0f), //between coms-office
            new Vector3(17.5f, -25.7f, 0.0f), //snowman under office
        ];
    }

    public class MiraData : MapData
    {
        static private Vector2[] MapPositions =
        [
        //ラウンチパッド
        new(-4.4f, 3.3f),
        //ランチパッド下通路
        new(3.7f, -1.7f),
        //メッドベイ
        new(15.2f, 0.4f),
        //コミュ
        new(14f, 4f),
        //三叉路
        new(12.3f, 6.7f), new(23.6f, 6.8f),
        //ロッカー
        new(9f, 5f), new(8.4f, 1.4f),
        //デコン
        new(6.0f, 5.6f),
        //デコン上通路
        new(6.0f, 11.6f),
        //リアクター
        new(2.5f, 10.3f), new(2.5f, 13f),
        //ラボラトリ
        new(7.6f, 13.9f), new(9.7f, 10.4f), new(10.7f, 12.2f),
        //カフェ
        new(21.8f, 5f), new(10.7f, 12.2f), new(28.3f, 0.2f), new(25.5f, 2.3f), new(22.1f, 2.6f),
        //ストレージ
        new(19.2f, 1.7f), new(18.5f, 4.2f),
        //バルコニー
        new(18.3f, -3.2f), new(23.7f, -1.9f),
        //三叉路上通路
        new(17.8f, 19f),
        //オフィス
        new(15.7f, 17.2f), new(13.7f, 20.4f), new(13.6f, 18.7f),
        //アドミン
        new(20.6f, 20.8f), new(22.3f, 18.6f), new(21.2f, 17.3f), new(19.4f, 17.6f),
        //グリーンハウス
        new(13.2f, 22.3f), new(22.4f, 23.3f), new(20.2f, 24.3f), new(16.5f, 24.4f), new(20.7f, 22.2f), new(18f, 25.3f),
        ];

        protected override Vector2[] MapArea => MapPositions;

        public override Vector3[] SpawnPos =>
        [
            new Vector3(-4.5f, 3.5f, 0.0f), //launchpad top
            new Vector3(-4.5f, -1.4f, 0.0f), //launchpad bottom
            new Vector3(8.5f, -1f, 0.0f), //launchpad- med hall
            new Vector3(14f, -1.5f, 0.0f), //medbay
            new Vector3(16.5f, 3f, 0.0f), // comms
            new Vector3(10f, 5f, 0.0f), //lockers
            new Vector3(6f, 1.5f, 0.0f), //locker room
            new Vector3(2.5f, 13.6f, 0.0f), //reactor
            new Vector3(6f, 12f, 0.0f), //reactor middle
            new Vector3(9.5f, 13f, 0.0f), //lab
            new Vector3(15f, 9f, 0.0f), //bottom left cross
            new Vector3(17.9f, 11.5f, 0.0f), //middle cross
            new Vector3(14f, 17.3f, 0.0f), //office
            new Vector3(19.5f, 21f, 0.0f), //admin
            new Vector3(14f, 24f, 0.0f), //greenhouse left
            new Vector3(22f, 24f, 0.0f), //greenhouse right
            new Vector3(21f, 8.5f, 0.0f), //bottom right cross
            new Vector3(28f, 3f, 0.0f), //caf right
            new Vector3(22f, 3f, 0.0f), //caf left
            new Vector3(19f, 4f, 0.0f), //storage
            new Vector3(22f, -2f, 0.0f), //balcony
        ];
    }

    public class SkeldData : MapData
    {
        static private Vector2[] MapPositions =
        [
        //カフェ
        new(0f, 5.3f), new(-5.2f, 1.2f), new(-0.9f, -3.1f), new(4.6f, 1.2f),
        //ウェポン
        new(10.1f, 3f),
        //コの字通路/O2
        new(9.6f, -3.4f), new(11.8f, -6.5f),
        //ナビ
        new(16.7f, -4.8f),
        //シールド
        new(9.3f, -10.3f), new(9.5f, -14.1f),
        //コミュ上
        new(5.2f, -12.2f),
        //コミュ
        new(3.8f, -15.4f),
        //ストレージ
        new(-0.3f, -9.8f), new(-0.28f, -16.4f), new(-4.5f, -14.3f),
        //エレク
        new(-9.6f, -11.3f), new(-7.5f, -8.4f),
        //ロアエンジン右
        new(-12.1f, -11.4f),
        //ロアエンジン
        new(-15.4f, -13.1f), new(-16.8f, -9.8f),
        //アッパーエンジン
        new(-16.8f, -1f), new(-15.2f, 2.4f),
        //セキュ
        new(-13.8f, -4.5f),
        //リアクター
        new(-20.9f, -5.4f),
        //メッドベイ
        new(-7.3f, -4.6f), new(-9.2f, -2.1f),
        //アドミン
        new(2.6f, -7.1f), new(6.3f, -9.5f)
        ];
        protected override Vector2[] MapArea => MapPositions;

        public override Vector3[] SpawnPos =>
        [
            new Vector3(-2.2f, 2.2f, 0.0f), //cafeteria. botton. top left.
            new Vector3(0.7f, 2.2f, 0.0f), //caffeteria. button. top right.
            new Vector3(-2.2f, -0.2f, 0.0f), //caffeteria. button. bottom left.
            new Vector3(0.7f, -0.2f, 0.0f), //caffeteria. button. bottom right.
            new Vector3(10.0f, 3.0f, 0.0f), //weapons top
            new Vector3(9.0f, 1.0f, 0.0f), //weapons bottom
            new Vector3(6.5f, -3.5f, 0.0f), //O2
            new Vector3(11.5f, -3.5f, 0.0f), //O2-nav hall
            new Vector3(17.0f, -3.5f, 0.0f), //navigation top
            new Vector3(18.2f, -5.7f, 0.0f), //navigation bottom
            new Vector3(11.5f, -6.5f, 0.0f), //nav-shields top
            new Vector3(9.5f, -8.5f, 0.0f), //nav-shields bottom
            new Vector3(9.2f, -12.2f, 0.0f), //shields top
            new Vector3(8.0f, -14.3f, 0.0f), //shields bottom
            new Vector3(2.5f, -16f, 0.0f), //coms left
            new Vector3(4.2f, -16.4f, 0.0f), //coms middle
            new Vector3(5.5f, -16f, 0.0f), //coms right
            new Vector3(-1.5f, -10.0f, 0.0f), //storage top
            new Vector3(-1.5f, -15.5f, 0.0f), //storage bottom
            new Vector3(-4.5f, -12.5f, 0.0f), //storrage left
            new Vector3(0.3f, -12.5f, 0.0f), //storrage right
            new Vector3(4.5f, -7.5f, 0.0f), //admin top
            new Vector3(4.5f, -9.5f, 0.0f), //admin bottom
            new Vector3(-9.0f, -8.0f, 0.0f), //elec top left
            new Vector3(-6.0f, -8.0f, 0.0f), //elec top right
            new Vector3(-8.0f, -11.0f, 0.0f), //elec bottom
            new Vector3(-12.0f, -13.0f, 0.0f), //elec-lower hall
            new Vector3(-17f, -10f, 0.0f), //lower engine top
            new Vector3(-17.0f, -13.0f, 0.0f), //lower engine bottom
            new Vector3(-21.5f, -3.0f, 0.0f), //reactor top
            new Vector3(-21.5f, -8.0f, 0.0f), //reactor bottom
            new Vector3(-13.0f, -3.0f, 0.0f), //security top
            new Vector3(-12.6f, -5.6f, 0.0f), // security bottom
            new Vector3(-17.0f, 2.5f, 0.0f), //upper engibe top
            new Vector3(-17.0f, -1.0f, 0.0f), //upper engine bottom
            new Vector3(-10.5f, 1.0f, 0.0f), //upper-mad hall
            new Vector3(-10.5f, -2.0f, 0.0f), //medbay top
            new Vector3(-6.5f, -4.5f, 0.0f) //medbay bottom
        ];
    }
}
