class AuthSession {
  const AuthSession(this.accessToken, this.refreshToken);

  final String accessToken;
  final String refreshToken;
}
