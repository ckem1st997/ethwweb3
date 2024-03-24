using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Web3;

namespace eth_wwallet
{
    internal class Program
    {
        // Biến để cache dữ liệu
        private static List<string> cachedData = new List<string>();
        private static HashSet<string> addData = new HashSet<string>();
        private static Web3 web3 = new Web3("https://bsc-dataseed.binance.org/");
        static async Task Main(string[] args)
        {

            string currentDirectory = Environment.CurrentDirectory;
            //  string projectRootDirectory1 = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
            string filePath1 = Path.Combine(currentDirectory, "words_alpha.txt");
            // string filePath1 = Path.Combine(projectRootDirectory1, "words_alpha.txt");
            // string filePath2 = Path.Combine(projectRootDirectory1, "eth-list-address.txt");
            string filePath2 = Path.Combine(currentDirectory, "eth-list-address.txt");
            Console.WriteLine(filePath2);
            List<string> data = await GetDataAsync(filePath1);
            List<string> rd = new List<string>();

            string mnemonicWords = "";
            int count = 0;
            int seedNum = 12;

            Random random = new Random();
            while (true)
            {

                rd = new List<string>();
                var listRd = new List<int>();
                mnemonicWords = string.Empty;
                for (int i = 0; i < seedNum; i++)
                {
                    bool b = true;
                    while (b)
                    {
                        int randomIndex = random.Next(2048);
                        var check = listRd.Where(x => x == randomIndex);
                        if ((check == null || !check.Any()))
                        {
                            rd.Add(randomIndex.ToString());
                            listRd.Add(randomIndex);
                            mnemonicWords = mnemonicWords + " " + data[randomIndex];
                            b = false;
                        }
                    }

                }
                mnemonicWords = mnemonicWords.Trim();
                if (!(!string.IsNullOrEmpty(mnemonicWords) && (mnemonicWords.Split(" ").Length == 12 || mnemonicWords.Split(" ").Length == 24))) continue;
                try
                {
                    count++;
                    var listAddress = new List<string>();

                    // Tạo một ví mới từ seed
                    Wallet wallet = new Wallet(mnemonicWords, null);
                    string accountAddress44 = wallet.GetAccount(0).Address;
                    if (!string.IsNullOrEmpty(accountAddress44))
                    {
                        listAddress.Add(accountAddress44);
                    }
                    Console.WriteLine($"[{count}]-{accountAddress44}");
                    // Tạo và kiểm tra các loại địa chỉ khác nhau
                    await DeriveAndCheckBalance(accountAddress44, filePath2, mnemonicWords).ConfigureAwait(false);

                    //_ = Task.Run(async () =>
                    //{
                    //    await DeriveAndCheckBalance(accountAddress44, filePath2, mnemonicWords).ConfigureAwait(false);
                    //});
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }


        }


        static async Task DeriveAndCheckBalance(string listAddress, string csvFilePath, string mnemonicWords)
        {
            try
            {
                var balance = await web3.Eth.GetBalance.SendRequestAsync(listAddress);
                var getBalanceTask = Web3.Convert.FromWei(balance.Value);
                if (getBalanceTask > 0)
                {
                    string output = $"12 Seed: {mnemonicWords} | address:{String.Join(", ", listAddress)}";

                    string currentDirectory = Environment.CurrentDirectory;
                    string projectRootDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
                    string filePath = Path.Combine(projectRootDirectory, "btc-wallet.txt");

                    await using (StreamWriter sw = File.AppendText(filePath))
                    {
                        await sw.WriteLineAsync(output);
                    }
                    Console.WriteLine($"Thông tin đã được ghi vào file cho địa chỉ: {String.Join(", ", listAddress)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        static async Task<List<string>> GetDataAsync(string filePath)
        {
            // Nếu dữ liệu đã được cache, trả về dữ liệu từ cache
            if (cachedData != null && cachedData.Count > 0)
            {
                Console.WriteLine("Lấy dữ liệu từ cache.");
                return cachedData;
            }

            // Nếu chưa có dữ liệu trong cache, đọc từ file
            Console.WriteLine("Đọc dữ liệu từ file và cache nó.");
            cachedData = new List<string>();

            // Kiểm tra xem file có tồn tại không
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File không tồn tại.");
                return cachedData;
            }

            // Đọc file và lưu vào cache
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cachedData.Add(line);
                }
            }

            return cachedData;
        }

    }
}
