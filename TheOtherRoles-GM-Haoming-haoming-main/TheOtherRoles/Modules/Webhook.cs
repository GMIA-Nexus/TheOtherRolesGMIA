using System.Collections.Generic;
using System.Net.Http;
using System.Text;


namespace TheOtherRoles
{
    class Webhook
    {
        private static HttpClient client = new HttpClient();

        public static string colorIdToEmoji(int colorId)
        {
            switch (colorId)
            {
                case 0:
                    return "<:aured:840792952456740885>";
                case 1:
                    return "<:aublue:840792952565137408>";
                case 2:
                    return "<:augreen:840792952535646239>";
                case 3:
                    return "<:aupink:840792952690704435>";
                case 4:
                    return "<:auorange:840792952687165480>";
                case 5:
                    return "<:auyellow:840792952489508876>";
                case 6:
                    return "<:aublack:840792952292376588>";
                case 7:
                    return "<:auwhite:840792952800149524>";
                case 8:
                    return "<:aupurple:840792952304435234>";
                case 9:
                    return "<:aubrown:840792952660951050>";
                case 10:
                    return "<:aucyan:840792952259084310>";
                case 11:
                    return "<:aulime:840792952741953536>";
                case 12:
                    return "<:aumaroon:854970124373065728>";
                case 13:
                    return "<:aurose:854970124309233684>";
                case 14:
                    return "<:aubanana:854970123834884118>";
                case 15:
                    return "<:augrey:840792952682971147>";
                case 16:
                    return "<:autan:854973543484751883>";
                case 17:
                    return "<:aucoral:854970124037390337>";
                case 18:
                    return "<:ausalmon:840792952698830868>";
                case 19:
                    return "<:aubordeaux:840792952682577940>";
                case 20:
                    return "<:auolive:840792952404967425>";
                case 21:
                    return "<:auturqoise:840792952498421802>";
                case 22:
                    return "<:aumint:840792952347164673>";
                case 23:
                    return "<:aulavender:840792952661082112>";
                case 24:
                    return "<:aunougat:840792952628051978>";
                case 25:
                    return "<:auwasabi:840792954057523200>";
                case 26:
                    return "<:auhotpink:840792952573394944>";
                case 27:
                    return "<:aupetrol:840792952762925106>";
                case 28:
                    return "<:aulemon:854970124300582962>";
                case 29:
                    return "<:ausunrise:854970124296519711>";
                case 30:
                    return "<:auteal:854970124297437225>";
                case 31:
                    return "<:aublurple:854970124330074142>";
                case 32:
                    return "<:ausunrise:854970124296519711>";
                case 33:
                    return "<:auice:854970124293373952>";
                default:
                    return "<:aured:840792952456740885>";


            }
        }
        public static void post(List<Dictionary<string, object>> embeds, string bonusText, string extraText)
        {
            if (TheOtherRolesPlugin.WebhookUrl.Value != "" && bonusText != "forceEnd")
            {
                var value = new Dictionary<string, object>();
                value.Add("content", "試合結果 " + ModTranslation.getString(bonusText) + " " + ModTranslation.getString(extraText));
                if (bonusText == "impostorWin")
                {
                    embeds[0].Add("color", 16711680);
                }
                else if (bonusText == "crewWin")
                {
                    embeds[0].Add("color", 8375523);
                }
                value.Add("embeds", embeds);

                // json変換
                string data = Helpers.SerializeObject(value);
                Patches.Logger.info(data);

                var content = new StringContent(data, Encoding.UTF8, "application/json");

                var response = client.PostAsync(TheOtherRolesPlugin.WebhookUrl.Value, content).Result;
            }
        }
    }
}
