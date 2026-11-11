import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { registerDevice } from '@/api/notifications.api';
import { isNativeApp, native } from './native';

/**
 * Push wiring for the console when it runs inside the Android shell.
 *
 * The shell owns Firebase (it is the thing with a notification tray). It hands the console
 * two events:
 *
 *   native:pushToken  { token, platform, device, appVersion }
 *       Fired on launch and whenever Firebase rotates the token. We forward it to the API so
 *       the platform has somewhere to push. Nothing here worked before: the shell had no
 *       Firebase at all and the API's UpdateFCMToken was a stub.
 *
 *   native:push       { notificationId, title, body, url, foreground }
 *       A push arrived, or the user tapped one. We refresh the bell immediately so the badge
 *       is right without waiting for the next poll, and navigate if the alert carries a path.
 *
 * In a plain browser every one of these is a no-op — the same code ships to both.
 */

const TOKEN_SENT_KEY = 'fino.push.token';

/** Remember the last token we registered, so a relaunch is not a pointless round trip. */
function alreadyRegistered(token: string): boolean {
  try {
    return localStorage.getItem(TOKEN_SENT_KEY) === token;
  } catch {
    return false;
  }
}

function remember(token: string): void {
  try {
    localStorage.setItem(TOKEN_SENT_KEY, token);
  } catch {
    /* private mode — re-registering next launch is harmless */
  }
}

/** Forget the registered token so the next sign-in re-registers this device. */
export function forgetPushToken(): void {
  try {
    localStorage.removeItem(TOKEN_SENT_KEY);
  } catch {
    /* ignore */
  }
}

/**
 * Tell the API to stop pushing to this device. Called on sign-out, so the next person to
 * use the handset does not receive the previous user's alerts.
 */
export async function unregisterThisDevice(): Promise<void> {
  try {
    const token = localStorage.getItem(TOKEN_SENT_KEY);
    if (!token) return;
    await registerDevice({ fcmToken: token, platform: 'android', remove: true });
    forgetPushToken();
  } catch {
    /* best effort — a failed unregister must never block signing out */
  }
}

interface PushTokenDetail {
  token?: string;
  platform?: 'android' | 'web' | 'ios';
  device?: string;
  appVersion?: string;
}

interface PushDetail {
  notificationId?: number | string;
  url?: string;
  foreground?: boolean;
}

/**
 * Wire the shell's push events to the API and the bell. Mount once, inside the authenticated
 * shell (AppShell / MobileShell both render under it).
 */
export function usePushBridge(enabled: boolean, navigate?: (to: string) => void): void {
  const qc = useQueryClient();

  useEffect(() => {
    if (!enabled) return;

    async function onToken(event: Event) {
      const detail = (event as CustomEvent<PushTokenDetail>).detail ?? {};
      const token = (detail.token ?? '').trim();
      if (!token || alreadyRegistered(token)) return;

      const res = await registerDevice({
        fcmToken: token,
        platform: detail.platform ?? 'android',
        deviceName: detail.device ?? '',
        appVersion: detail.appVersion ?? '',
      });
      if (res.ok) remember(token);
    }

    function onPush(event: Event) {
      const detail = (event as CustomEvent<PushDetail>).detail ?? {};
      // The alert is already stored server-side; re-reading the feed is what makes the badge
      // and the list agree, whether the app was open or was just opened by the tap.
      qc.invalidateQueries({ queryKey: ['notification-feed'] });
      const url = (detail.url ?? '').trim();
      if (url && !detail.foreground && navigate) navigate(url);
    }

    window.addEventListener('native:pushToken', onToken);
    window.addEventListener('native:push', onPush);

    // Ask the shell for the token it already has (the event may have fired before mount) and,
    // on Android 13+, for tray permission if it has not been granted yet.
    if (isNativeApp()) {
      try {
        native()?.notifications?.();
        (native() as unknown as { requestPushToken?: () => void })?.requestPushToken?.();
      } catch {
        /* older shell build without the push bridge — the bell still works */
      }
    }

    return () => {
      window.removeEventListener('native:pushToken', onToken);
      window.removeEventListener('native:push', onPush);
    };
  }, [enabled, qc, navigate]);
}
