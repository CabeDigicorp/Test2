using Commons;
using ModelData.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebServiceClient;

namespace MainApp
{
    public class WebLoginView : NotificationBase
    {
        public void Init(LoginDto loginDto)
        {
            //CodiceCliente = loginDto.CodiceCliente;
            Email = loginDto.Email;
            PasswordText = String.Empty;
            IsPasswordRevealed = false;
            RememberMe = true;

            LoginDto = null;
        }

        public LoginDto LoginDto { get; protected set; } = null;


        string _codiceCliente = string.Empty;
        public string CodiceCliente { get => _codiceCliente; set => SetProperty(ref _codiceCliente, value); }

        string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        string _passwordText = string.Empty;
        public string PasswordText  { get => _passwordText; set => SetProperty(ref _passwordText, value); }

        bool _isPasswordRevealed = false;
        public bool IsPasswordRevealed
        {
            get => _isPasswordRevealed;
            set 
            { 
                if (SetProperty(ref _isPasswordRevealed, value))
                {
                    RaisePropertyChanged(GetPropertyName(() => PasswordText));
                }
            }
        }

        bool _rememberMe = false;
        public bool RememberMe { get => _rememberMe; set => SetProperty(ref _rememberMe, value); }

        public void Accedi()
        {
            LoginDto = new LoginDto();

            //LoginDto.CodiceCliente = CodiceCliente;
            LoginDto.Email = Email;
            LoginDto.Password = PasswordText;
            LoginDto.RememberMe = RememberMe;
        }
    }
}
