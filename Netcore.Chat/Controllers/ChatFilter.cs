using Microsoft.AspNetCore.Hosting;
using Netcore.Chat.Interfaces;
using Netcore.Chat.Models;
using NetCore.Utils.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace Netcore.Chat.Controllers
{
    public class ChatFilter
    {
        private readonly object lockLoadBadWords = new object();
        private readonly object lockLoadBadLinks = new object();
        private readonly object lockLoadBanUsers = new object();
        private readonly ISQLAccess _sql;

        private readonly string RegexAcceptChars = @"[^aáàảãạăắằẳẵặâấầẩẫậđeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴ\w\s\d]+";
        private List<string> BadLinks = new List<string>();
        private List<string> BanUsers = new List<string>();
        public List<Admin> ADMINS = new List<Admin>();
        public List<BadWord> BADWORDS = new List<BadWord>();
        private List<KeywordReplace> KeywordReplace = new List<KeywordReplace>();
        private List<BlockAccount> ListBlockAccount = new List<BlockAccount>();
        private ConcurrentDictionary<string, string> ReplaceVNs = new ConcurrentDictionary<string, string>();
        private IHostingEnvironment _env;

        public ChatFilter(IHostingEnvironment env, ISQLAccess sql)
        {
            _env = env;
            _sql = sql;
            LoadData();
            var aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;
        }

        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            LoadAdmins();
            LoadListBadWords();
            LoadBlockAccounts();
            LoadKeywordReplaces();
        }

        public bool IsAdmin(string userName)
        {
            return ADMINS.Any(ad => ad.AccountName == userName);
        }

        public string RemoveBadWords(string input)
        {
            if (BADWORDS == null || BADWORDS.Count <= 0)
            {
                LoadListBadWords();
            }
            int bwLength = BADWORDS.Count;
            for (int i = 0; i < bwLength; i++)
            {
                try
                {
                    input = input.Replace(BADWORDS[i].text, "***", StringComparison.CurrentCultureIgnoreCase);
                }
                catch (Exception ex)
                {
                    NLogManager.LogException(ex);
                }
            }

            return input;
        }

        public string ReplaceKeyword(string input)
        {
            if (BADWORDS == null || BADWORDS.Count <= 0)
            {
                LoadListBadWords();
            }

            if (KeywordReplace.Count > 0)
            {
                foreach (var item in KeywordReplace)
                {
                    if (string.IsNullOrEmpty(item.text) || string.IsNullOrEmpty(item.replace))
                    {
                        continue;
                    }
                    input = input.Replace(item.text, item.replace, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return input;
        }

        public bool CheckBanUsers(ChatUser chatUser)
        {
            if (ListBlockAccount == null || ListBlockAccount.Count <= 0)
            {
                LoadBlockAccounts();
            }
            if (ListBlockAccount.Count > 0)
            {
                foreach (var item in ListBlockAccount)
                {
                    if (item.name == chatUser.UserName || item.accountid == chatUser.AccountID)
                    {
                        if (DateTime.Now >= item.endtimeblock)
                        {
                            //DeleteAccountBlock(item.key);
                            _sql.DeleteBlockedAccount(item.id);
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public bool CheckBanUsers(long id)
        {
            if (ListBlockAccount == null || ListBlockAccount.Count <= 0)
            {
                LoadBlockAccounts();
            }
            if (ListBlockAccount.Count > 0)
            {
                foreach (var item in ListBlockAccount)
                {
                    if (item.id == id)
                    {
                        if (DateTime.Now >= item.endtimeblock)
                        {
                            //DeleteAccountBlock(item.key);
                            _sql.DeleteBlockedAccount(item.id);
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public string CutOff(string input, string pattern = " ")
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                input = input.Replace(pattern[i].ToString(), "");
            }

            return input;
        }

        public string ReplaceVN(string input)
        {
            if (ReplaceVNs == null || ReplaceVNs.Count < 1)
            {
                if (Monitor.TryEnter(ReplaceVNs, 60000))
                {
                    try
                    {
                        ReplaceVNs.TryAdd("[@ÅÄäẢÃÁÀẠảãáàạÂĂẨẪẤẦẬẩẫấầậẲẴẮẰẶẳẵắằặ]+", "a");
                        ReplaceVNs.TryAdd("[ß]+", "b");
                        ReplaceVNs.TryAdd("[Ç€]+", "c");
                        ReplaceVNs.TryAdd("[ËẺẼÉÈẸẻẽéèẹÊỂỄẾỀỆêểễếềệ]+", "e");
                        ReplaceVNs.TryAdd("[ÏιỈĨÍÌỊỉĩíìị]+", "i");
                        ReplaceVNs.TryAdd("[ØÖöΘ☻❂ỎÕÓÒỌỏõóòọÔỔỖỐỒỘôổỗốồộƠỞỠỚỜỢơởỡớờợ0]+", "o");
                        ReplaceVNs.TryAdd("[Šš]+", "s");
                        ReplaceVNs.TryAdd("[τ]+", "t");
                        ReplaceVNs.TryAdd("[ÜỦŨÙỤÚủũúùụỬỮỨỪỰửữứừự]+", "u");
                        ReplaceVNs.TryAdd("[•,;:]+", ".");
                    }
                    finally
                    {
                        Monitor.Exit(ReplaceVNs);
                    }
                }
            }

            foreach (string key in ReplaceVNs.Keys)
            {
                try
                {
                    Regex regx = new Regex(key, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    input = regx.Replace(input, ReplaceVNs[key]);

                    //NLogManager.LogInfo(key + " : " + input);
                }
                catch (Exception ex)
                {
                    NLogManager.LogException(ex);
                }
            }

            return input;
        }

        public void LoadAdmins()
        {
            ADMINS = _sql.LoadListAdmin();
        }

        public void LoadListBadWords()
        {
            BADWORDS = _sql.LoadListBadWord();
        }

        public void LoadKeywordReplaces()
        {
            KeywordReplace = _sql.LoadListKeywordReplace();
        }

        public void LoadBlockAccounts()
        {
            ListBlockAccount = _sql.LoadListBlockAccount();
        }

        public string RemoveUnAcceptChars(string input)
        {
            Regex regx = new Regex(RegexAcceptChars, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string output = regx.Replace(input, "*");

            return output;
        }

        public string RemoveBadLinks(string input, out bool Flag)
        {
            input = CutOff(input, " '`~");
            input = ReplaceVN(input);

            Flag = false;
            int bwLength = BadLinks.Count;
            for (int i = 0; i < bwLength; i++)
            {
                try
                {
                    string bl = BadLinks[i];
                    if (bl.StartsWith("regex::", StringComparison.OrdinalIgnoreCase))
                    {
                        bl = bl.Substring(7);
                        Regex regx = new Regex(bl, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        if (regx.IsMatch(input))
                            Flag = true;
                        input = regx.Replace(input, "*");
                    }
                    else
                    {
                        int countLength = input.Length;
                        input = input.Replace(bl, "*", StringComparison.OrdinalIgnoreCase);
                        if (input.Length != countLength)
                        {
                            Flag = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.LogException(ex);
                }
            }

            return input;
        }

        public string ReturnCheckBanUsers(string username)
        {
            string message = "";
            if (ListBlockAccount.Count > 0)
            {
                foreach (var item in ListBlockAccount)
                {
                    if (item.name == username)
                    {
                        message = string.Format("Tài khoản đang bị Block, lý do: {0} - Thời hạn đến: {1}", item.namereasonblock, item.endtimeblock);
                    }
                }
            }
            return message;
        }

        //public bool BanUser(string username)
        //{
        //    if (string.IsNullOrEmpty(username))
        //        return false;

        //    if (Monitor.TryEnter(lockLoadBanUsers, 60000))
        //    {
        //        try
        //        {
        //            if (CheckBanUsers(username))
        //                return true;

        //            File.AppendAllText(BANUSERS_FILE, Environment.NewLine + username);

        //            BanUsers.Add(username);
        //            NLogManager.LogInfo(string.Format("Admins has been banned user: username={0}", username));

        //            return true;
        //        }
        //        finally
        //        {
        //            Monitor.Exit(lockLoadBanUsers);
        //        }
        //    }
        //    return false;
        //}

        //public bool AddBadLink(string link)
        //{
        //    if (Monitor.TryEnter(lockLoadBadLinks, 60000))
        //    {
        //        try
        //        {
        //            File.AppendAllText(BADLINKS_FILE, Environment.NewLine + link);

        //            BadLinks.Add(link);
        //            NLogManager.LogInfo(string.Format("Admins has been added bad link: link={0}", link));

        //            return true;
        //        }
        //        finally
        //        {
        //            Monitor.Exit(lockLoadBadLinks);
        //        }
        //    }

        //    return false;
        //}

        //public bool AddBadWord(string word)
        //{
        //    if (Monitor.TryEnter(lockLoadBadWords, 60000))
        //    {
        //        try
        //        {
        //            File.AppendAllText(BADWORDS_FILE, Environment.NewLine + word);

        //            BadWords.Add(word);
        //            NLogManager.LogInfo(string.Format("Admins has been added bad word: word={0}", word));

        //            return true;
        //        }
        //        finally
        //        {
        //            Monitor.Exit(lockLoadBadWords);
        //        }
        //    }

        //    return false;
        //}

        #region [Xử lý XML - huandh 2016.03.25]

        //private void LoadBlackList()
        //{
        //    XDocument xmldoc = XDocument.Load(_env.ContentRootPath + BLACKLIST_FILE);
        //    IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
        //    BadWords = new List<string>();
        //    foreach (XElement xe in q)
        //    {
        //        BadWords.Add(xe.Attribute("text").Value);
        //    }
        //}
        //private void LoadKeywordReplace()
        //{
        //    try
        //    {
        //        string key = "KEYWORDREPLACE_FILE";
        //        XDocument xmldoc = XDocument.Load(_env.ContentRootPath + KEYWORDREPLACE_FILE);
        //        IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
        //        var Data = new List<ObjKeywordReplace>();
        //        foreach (XElement xe in q)
        //        {
        //            Data.Add(new ObjKeywordReplace
        //            {
        //                text = xe.Attribute("text").Value,
        //                replace = xe.Attribute("replace").Value
        //            });

        //        }
        //        KeywordReplace = Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogException(ex);
        //    }

        //}
        //private void LoadAccountBlock()
        //{
        //    try
        //    {
        //        string mappath = _env.ContentRootPath + ACCOUNTBLOCK_FILE;
        //        XDocument xmldoc = XDocument.Load(mappath);
        //        IEnumerable<XElement> q = from xe in xmldoc.Descendants("key") select xe;
        //        List<ListAccountBlock> currAccountBlocks = new List<ListAccountBlock>();
        //        foreach (XElement xe in q)
        //        {
        //            currAccountBlocks.Add(new ListAccountBlock
        //            {
        //                key = xe.Attribute("key").Value,
        //                name = xe.Attribute("name").Value,
        //                accountid = xe.Attribute("accountid").Value,
        //                reasonblock = xe.Attribute("reasonblock").Value,
        //                namereasonblock = xe.Attribute("namereasonblock").Value,
        //                typeblock = xe.Attribute("typeblock").Value,
        //                endtimeblock = xe.Attribute("endtimeblock").Value,
        //                createDate = xe.Attribute("createDate").Value,

        //            });
        //        }

        //        ListAccountBlock = currAccountBlocks;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogException(ex);
        //    }

        //}
        //private void DeleteAccountBlock(string key)
        //{
        //    try
        //    {
        //        XDocument xmldoc = XDocument.Load(_env.ContentRootPath + ACCOUNTBLOCK_FILE);
        //        XElement xmlelement = xmldoc.Element("AccountBlock").Elements("key").Single(x => (string)x.Attribute("key") == key);
        //        xmlelement.Remove();
        //        xmldoc.Save(Path.Combine(_env.WebRootPath, ACCOUNTBLOCK_FILE));
        //        LoadAccountBlock();
        //    }
        //    catch (Exception ex)
        //    { }

        //}

        #endregion [Xử lý XML - huandh 2016.03.25]
    }
}