import '../../core/result.dart';
import '../contracts/auth_repository.dart';
import '../entities/auth_session.dart';

class LoginUseCase {
  const LoginUseCase(this._authRepository);

  final AuthRepository _authRepository;

  AsyncResult<AuthSession> execute(String email, String password) async {
    if (email.trim().isEmpty || password.trim().isEmpty) {
      return Failure<AuthSession>(AppException.business('invalidCredentials'));
    }

    final result = await _authRepository.login(email, password);
    return result;
  }
}
