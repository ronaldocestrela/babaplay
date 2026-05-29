import '../../../domain/auth/contracts/auth_repository.dart';
import '../../../domain/auth/entities/auth_session.dart';
import '../../../domain/core/result.dart';

class AuthRepositoryImpl implements AuthRepository {
  @override
  AsyncResult<AuthSession> login(String email, String password) async {
    if (email == 'admin@babaplay.com' && password == '123456') {
      return const Success<AuthSession>(
        AuthSession('mock-access-token', 'mock-refresh-token'),
      );
    }

    return Failure<AuthSession>(
      AppException.business('INVALID_CREDENTIALS'),
    );
  }

  @override
  AsyncResult<Unit> logout(String refreshToken) async {
    if (refreshToken.trim().isEmpty) {
      return Failure<Unit>(AppException.business('INVALID_TOKEN'));
    }

    return const Success<Unit>(unit);
  }
}
