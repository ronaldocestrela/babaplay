const KEY = 'pending-association-invite-token'

export function setPendingInviteToken(token: string) {
  sessionStorage.setItem(KEY, token)
}

export function getPendingInviteToken() {
  return sessionStorage.getItem(KEY)
}

export function clearPendingInviteToken() {
  sessionStorage.removeItem(KEY)
}
