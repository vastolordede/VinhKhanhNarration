import { FormEvent, useEffect, useMemo, useState } from 'react';
import { getList } from '../../api/crud';
import { endpoints } from '../../api/endpoints';
import { getApiError } from '../../api/http';
import {
  getNarrations,
  getTranslations,
  updateTranslation
} from '../../api/narrationApi';
import { PageHeader } from '../../components/layout/PageHeader';
import { Button } from '../../components/ui/Button';
import { Card } from '../../components/ui/Card';
import { Modal } from '../../components/ui/Modal';
import { Input } from '../../components/ui/Input';
import { Textarea } from '../../components/ui/Textarea';
import {
  LanguageDTO,
  NarrationContentDTO,
  NarrationTranslationDTO
} from '../../types';

export default function TranslationManagementScreen() {
  const [items, setItems] = useState<NarrationTranslationDTO[]>([]);
  const [narrations, setNarrations] = useState<NarrationContentDTO[]>([]);
  const [languages, setLanguages] = useState<LanguageDTO[]>([]);
  const [editing, setEditing] = useState<NarrationTranslationDTO | null>(null);
  const [title, setTitle] = useState('');
  const [text, setText] = useState('');
  const [busyId, setBusyId] = useState<number | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [translationList, narrationList, languageList] = await Promise.all([
        getTranslations(),
        getNarrations(),
        getList<LanguageDTO>(endpoints.languages)
      ]);
      setItems(translationList);
      setNarrations(narrationList);
      setLanguages(languageList);
    } catch (loadError) {
      setError(getApiError(loadError));
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const narrationMap = useMemo(
    () => new Map(narrations.map((x) => [x.narrationId, x])),
    [narrations]
  );
  const languageMap = useMemo(
    () => new Map(languages.map((x) => [x.languageId, x])),
    [languages]
  );

  function openEdit(item: NarrationTranslationDTO) {
    setEditing(item);
    setTitle(item.translatedTitle);
    setText(item.translatedText);
  }

  async function save(event: FormEvent) {
    event.preventDefault();
    if (!editing) return;

    setBusyId(editing.translationId);
    setError(null);
    setMessage(null);
    try {
      await updateTranslation(editing.translationId, {
        ...editing,
        translatedTitle: title.trim(),
        translatedText: text.trim()
      });
      setEditing(null);
      setMessage(
        'Đã cập nhật bản dịch. Audio cũ được đánh dấu Outdated; hãy tạo lại audio.'
      );
      await load();
    } catch (saveError) {
      setError(getApiError(saveError));
    } finally {
      setBusyId(null);
    }
  }


  return (
    <div>
      <PageHeader
        title="Translation Management"
        description="Bản dịch được tự tạo và tự duyệt sau khi Admin duyệt nội dung nguồn. Màn hình này dùng để kiểm tra hoặc chỉnh sửa khi cần."
      />

      {message && (
        <p className="mb-4 rounded-2xl bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700">
          {message}
        </p>
      )}
      {error && (
        <p className="mb-4 rounded-2xl bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-700">
          {error}
        </p>
      )}

      <div className="space-y-3">
        {items.map((item) => {
          const narration = narrationMap.get(item.narrationId);
          const language = languageMap.get(item.languageId);

          return (
            <Card key={item.translationId}>
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <p className="text-xs font-bold uppercase text-teal-700">
                    #{item.translationId} • {language?.languageName || item.languageId}
                    {' • '}{item.status}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    Narration: {narration?.title || item.narrationId}
                    {' • '}Provider: {item.provider || '-'}
                  </p>
                  <h2 className="mt-2 text-lg font-bold text-slate-900">
                    {item.translatedTitle}
                  </h2>
                  <p className="mt-2 whitespace-pre-line text-sm text-slate-600">
                    {item.translatedText}
                  </p>
                  {item.errorMessage && (
                    <p className="mt-2 text-sm font-semibold text-rose-600">
                      {item.errorMessage}
                    </p>
                  )}
                </div>

                <div className="flex flex-wrap gap-2">
                  <Button
                    variant="secondary"
                    onClick={() => openEdit(item)}
                    disabled={busyId === item.translationId}
                  >
                    Sửa bản dịch
                  </Button>
                </div>
              </div>
            </Card>
          );
        })}
      </div>

      <Modal
        open={Boolean(editing)}
        title="Chỉnh sửa bản dịch"
        onClose={() => setEditing(null)}
      >
        <form onSubmit={save} className="space-y-3">
          <Input
            required
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Tiêu đề đã dịch"
          />
          <Textarea
            required
            rows={9}
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Nội dung đã dịch"
          />
          <Button className="w-full">Lưu bản dịch</Button>
        </form>
      </Modal>
    </div>
  );
}
