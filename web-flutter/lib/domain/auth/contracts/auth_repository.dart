import '../../core/result.dart';
import '../entities/auth_session.dart';

abstract interface class AuthRepository {
  AsyncResult<AuthSession> login(String email, String password);
  AsyncResult<Unit> logout(String refreshToken);
}
