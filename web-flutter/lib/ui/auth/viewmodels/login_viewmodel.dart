import '../../../domain/auth/entities/auth_session.dart';
import '../../../domain/auth/usecases/login_usecase.dart';
import '../../../domain/core/result.dart';

sealed class LoginState {
  const LoginState();
}

final class LoginIdle extends LoginState {
  const LoginIdle();
}

final class LoginLoading extends LoginState {
  const LoginLoading();
}

final class LoginSuccess extends LoginState {
  const LoginSuccess(this.session);

  final AuthSession session;
}

final class LoginFailure extends LoginState {
  const LoginFailure(this.errorMessage);

  final String errorMessage;
}

class LoginViewModel {
  LoginViewModel(this._loginUseCase);

  final LoginUseCase _loginUseCase;
  LoginState _state = const LoginIdle();

  LoginState get state => _state;

  AsyncResult<AuthSession> login(String email, String password) async {
    _state = const LoginLoading();

    final result = await _loginUseCase.execute(email, password);

    return result.fold(
      (session) {
        _state = LoginSuccess(session);
        return Success<AuthSession>(session);
      },
      (exception) {
        _state = LoginFailure(exception.message);
        return Failure<AuthSession>(exception);
      },
    );
  }
}
