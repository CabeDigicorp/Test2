using System.Security.AccessControl;

namespace JoinWebUI.Utilities
{
    public static class DataStorageHelper
    {
        private static int GetDeterministicHashCode(string str)
        {
            try
            {
                unchecked
                {
                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    for (int i = 0; i < str.Length; i += 2)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ str[i];
                        if (i == str.Length - 1)
                            break;
                        hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                    }

                    return hash1 + (hash2 * 1566083941);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string GenerateKey(string? input)
        {
            try
            {
                int hashcode = 0;

                if (!string.IsNullOrEmpty(input))
                    hashcode = GetDeterministicHashCode(input);
                if (hashcode != 0)
                    return $"rtf_{hashcode}";
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Mi salva il session storage del tipo che gli passo, altrimenti un messaggio di errore e booleano falso.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static async Task<bool> SetSessionState<T>(Blazored.SessionStorage.ISessionStorageService SessionStorage, string element, T state)
        {
            try
            {
                await SessionStorage.SetItemAsync<T>(element, state);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossibile salvare la sessione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mi ritorna un elemento dalla session storage nel tipo che mi aspetto, altrimenti il default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static async Task<T?> GetSessionState<T>(Blazored.SessionStorage.ISessionStorageService SessionStorage, string element)
        {
            T? state = default(T);
            try
            {
                state = await SessionStorage.GetItemAsync<T>(element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossibile reperire la sessione: {ex.Message}");
                return default(T);
            }
            return state;
        }
    }
}
