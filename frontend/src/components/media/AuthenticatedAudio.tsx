import { useEffect, useState } from 'react';
import { getApiError, http } from '../../api/http';

type Props = {
  audioId: number;
  className?: string;
};

export function AuthenticatedAudio({ audioId, className }: Props) {
  const [source, setSource] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    let objectUrl: string | null = null;

    async function load() {
      setSource(null);
      setError(null);
      try {
        const response = await http.get<Blob>(`/api/secure-files/audio/${audioId}`, {
          responseType: 'blob',
          signal: controller.signal
        });
        objectUrl = URL.createObjectURL(response.data);
        setSource(objectUrl);
      } catch (loadError) {
        if (!controller.signal.aborted) setError(getApiError(loadError));
      }
    }

    void load();
    return () => {
      controller.abort();
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [audioId]);

  if (error) {
    return <p className="mt-2 text-xs font-semibold text-rose-600">Không tải được audio: {error}</p>;
  }
  if (!source) {
    return <p className="mt-2 text-xs text-slate-500">Đang tải audio…</p>;
  }
  return <audio className={className} controls preload="metadata" src={source} />;
}
