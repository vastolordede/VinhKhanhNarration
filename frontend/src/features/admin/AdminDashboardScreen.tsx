import { useEffect, useMemo, useState } from 'react';
import {
  BarChart3,
  CalendarRange,
  Headphones,
  MapPinned,
  MessageSquareText,
  Radio,
  Store,
  TicketCheck,
  Utensils,
  type LucideIcon
} from 'lucide-react';
import { endpoints } from '../../api/endpoints';
import { getApiError, http, unwrap } from '../../api/http';
import { PageHeader } from '../../components/layout/PageHeader';
import { Card } from '../../components/ui/Card';
import { useI18n } from '../../i18n/useI18n';
import {
  AdminDashboardActivityDTO,
  AdminDashboardStatisticsDTO,
  DashboardPeriod
} from '../../types';

type SummaryCard = {
  label: string;
  value: string | number;
  icon: LucideIcon;
  hint?: string;
};

type DistributionRow = {
  label: string;
  value: number;
  barClassName: string;
};

const periodOptions: Array<{ value: DashboardPeriod; vi: string; en: string }> = [
  { value: 'month', vi: 'Tháng này', en: 'This month' },
  { value: 'quarter', vi: 'Quý này', en: 'This quarter' },
  { value: 'halfYear', vi: '6 tháng gần nhất', en: 'Last 6 months' },
  { value: 'year', vi: '1 năm gần nhất', en: 'Last 12 months' },
  { value: 'all', vi: 'Toàn bộ', en: 'All time' }
];

const emptyStatistics: AdminDashboardStatisticsDTO = {
  period: 'month',
  from: '',
  to: '',
  places: 0,
  dishes: 0,
  narrations: 0,
  vendors: 0,
  feedbacks: 0,
  listening: 0,
  geofence: 0,
  guestSessions: 0,
  vendorPayments: 0,
  guestRevenue: 0,
  vendorRevenue: 0,
  totalRevenue: 0,
  activity: []
};

function getChartValue(item: AdminDashboardActivityDTO) {
  return Math.max(
    item.listening,
    item.geofence,
    item.feedbacks,
    item.guestSessions,
    0
  );
}

export default function AdminDashboardScreen() {
  const [period, setPeriod] = useState<DashboardPeriod>('month');
  const [statistics, setStatistics] =
    useState<AdminDashboardStatisticsDTO>(emptyStatistics);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { tx, uiLanguage } = useI18n();

  const isVietnamese = uiLanguage === 'vi';
  const text = (vi: string, en: string) => (isVietnamese ? vi : en);

  const money = useMemo(
    () =>
      new Intl.NumberFormat(isVietnamese ? 'vi-VN' : 'en-US', {
        style: 'currency',
        currency: 'VND',
        maximumFractionDigits: 0
      }),
    [isVietnamese]
  );

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    setError(null);

    http
      .get(endpoints.adminDashboardStatistics, { params: { period } })
      .then((response) => {
        if (!mounted) return;
        setStatistics(unwrap<AdminDashboardStatisticsDTO>(response));
      })
      .catch((loadError) => {
        if (!mounted) return;
        setError(getApiError(loadError));
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, [period]);

  const cards: SummaryCard[] = [
    { label: 'Places / POI', value: statistics.places, icon: MapPinned },
    { label: 'Dishes', value: statistics.dishes, icon: Utensils },
    { label: 'Narrations', value: statistics.narrations, icon: Radio },
    { label: 'Vendors', value: statistics.vendors, icon: Store },
    {
      label: 'Listening',
      value: statistics.listening,
      icon: Headphones,
      hint: text('Trong khoảng đã chọn', 'In selected period')
    },
    {
      label: 'Geofence Events',
      value: statistics.geofence,
      icon: BarChart3,
      hint: text('Log trong khoảng đã chọn', 'Logs in selected period')
    },
    {
      label: 'Feedback',
      value: statistics.feedbacks,
      icon: MessageSquareText,
      hint: text('Trong khoảng đã chọn', 'In selected period')
    },
    {
      label: text('Guest Session đã trả tiền', 'Paid Guest sessions'),
      value: statistics.guestSessions,
      icon: TicketCheck,
      hint: text('Mỗi payment tạo một session mới', 'One new session per payment')
    }
  ];

  const distributionRows: DistributionRow[] = [
    {
      label: text('Địa điểm / POI', 'Places / POI'),
      value: statistics.places,
      barClassName: 'bg-teal-600'
    },
    {
      label: text('Món ăn', 'Dishes'),
      value: statistics.dishes,
      barClassName: 'bg-emerald-500'
    },
    {
      label: text('Thuyết minh', 'Narrations'),
      value: statistics.narrations,
      barClassName: 'bg-cyan-500'
    },
    {
      label: 'Vendors',
      value: statistics.vendors,
      barClassName: 'bg-rose-500'
    },
    {
      label: text('Lượt nghe', 'Listening'),
      value: statistics.listening,
      barClassName: 'bg-indigo-500'
    },
    {
      label: 'Geofence',
      value: statistics.geofence,
      barClassName: 'bg-violet-500'
    },
    {
      label: 'Feedback',
      value: statistics.feedbacks,
      barClassName: 'bg-amber-500'
    },
    {
      label: text('Guest Session mới', 'New Guest sessions'),
      value: statistics.guestSessions,
      barClassName: 'bg-sky-500'
    }
  ];

  const maxDistributionValue = Math.max(
    ...distributionRows.map((row) => row.value),
    1
  );

  const maxActivity = Math.max(...statistics.activity.map(getChartValue), 1);

  const guestRevenuePercent =
    statistics.totalRevenue > 0
      ? (statistics.guestRevenue / statistics.totalRevenue) * 100
      : 0;

  function activityLabel(value: string) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;

    if (period === 'month') {
      return date.toLocaleDateString(isVietnamese ? 'vi-VN' : 'en-US', {
        day: '2-digit',
        month: '2-digit'
      });
    }

    if (period === 'all') return String(date.getFullYear());

    return date.toLocaleDateString(isVietnamese ? 'vi-VN' : 'en-US', {
      month: '2-digit',
      year: 'numeric'
    });
  }

  return (
    <div>
      <div className="flex flex-wrap items-start justify-between gap-4">
        <PageHeader
          title="Admin Dashboard"
          description="Tổng quan dữ liệu, lượt sử dụng và doanh thu theo khoảng thời gian."
        />

        <label className="flex items-center gap-2 rounded-2xl border border-slate-200 bg-white px-4 py-3 shadow-sm">
          <CalendarRange className="h-5 w-5 text-teal-700" />
          <select
            value={period}
            onChange={(event) => setPeriod(event.target.value as DashboardPeriod)}
            className="bg-transparent text-sm font-semibold text-slate-700 outline-none"
          >
            {periodOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {isVietnamese ? option.vi : option.en}
              </option>
            ))}
          </select>
        </label>
      </div>

      {error && (
        <p className="mb-5 rounded-2xl bg-rose-50 px-4 py-3 text-sm font-semibold text-rose-700">
          {error}
        </p>
      )}

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <Card key={card.label}>
              <div className="flex items-start justify-between gap-4">
                <div className="min-w-0">
                  <p className="text-sm text-slate-500">{tx(card.label)}</p>
                  <p className="mt-2 text-3xl font-bold text-slate-900">
                    {loading ? '—' : card.value}
                  </p>
                  {card.hint && (
                    <p className="mt-2 text-xs text-slate-400">{card.hint}</p>
                  )}
                </div>
                <div className="rounded-2xl bg-teal-50 p-3 text-teal-700">
                  <Icon className="h-5 w-5" />
                </div>
              </div>
            </Card>
          );
        })}
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-3">
        <Card className="xl:col-span-2">
          <div className="mb-5 flex items-center justify-between gap-4">
            <div>
              <h2 className="text-lg font-semibold text-slate-900">
                {text('Tổng quan dữ liệu và hoạt động', 'Data and activity overview')}
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                {text(
                  'Nội dung hiển thị tổng hiện có; hoạt động áp dụng theo bộ lọc.',
                  'Content shows current totals; activity follows the selected period.'
                )}
              </p>
            </div>
            <BarChart3 className="h-6 w-6 text-teal-700" />
          </div>

          <div className="space-y-4">
            {distributionRows.map((row) => (
              <div
                key={row.label}
                className="grid grid-cols-[112px_1fr_44px] items-center gap-3 sm:grid-cols-[150px_1fr_52px]"
              >
                <span className="truncate text-sm text-slate-600">{row.label}</span>
                <div className="h-3 overflow-hidden rounded-full bg-slate-100">
                  <div
                    className={`h-full rounded-full transition-all duration-500 ${row.barClassName}`}
                    style={{
                      width: `${Math.max(
                        row.value > 0
                          ? (row.value / maxDistributionValue) * 100
                          : 0,
                        row.value > 0 ? 4 : 0
                      )}%`
                    }}
                  />
                </div>
                <span className="text-right text-sm font-semibold text-slate-800">
                  {row.value}
                </span>
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <div className="mb-5">
            <h2 className="text-lg font-semibold text-slate-900">
              {text('Cơ cấu doanh thu', 'Revenue breakdown')}
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              {text('Guest Session và Vendor payment.', 'Guest Session and Vendor payments.')}
            </p>
          </div>

          <div className="flex flex-col items-center">
            <div
              className="relative h-44 w-44 rounded-full"
              style={{
                background:
                  statistics.totalRevenue > 0
                    ? `conic-gradient(#0f766e 0 ${guestRevenuePercent}%, #2563eb ${guestRevenuePercent}% 100%)`
                    : '#e2e8f0'
              }}
            >
              <div className="absolute inset-6 flex flex-col items-center justify-center rounded-full bg-white text-center">
                <span className="text-xs text-slate-500">{text('Tổng', 'Total')}</span>
                <strong className="mt-1 text-lg text-slate-900">
                  {money.format(statistics.totalRevenue)}
                </strong>
              </div>
            </div>

            <div className="mt-6 w-full space-y-3">
              <div className="rounded-2xl bg-teal-50 px-4 py-3">
                <div className="flex items-center justify-between gap-3">
                  <span className="text-sm font-semibold text-teal-900">
                    Guest Session
                  </span>
                  <strong className="text-sm text-teal-900">
                    {money.format(statistics.guestRevenue)}
                  </strong>
                </div>
                <p className="mt-1 text-xs text-teal-700">
                  {statistics.guestSessions} {text('session đã trả tiền', 'paid sessions')}
                </p>
              </div>

              <div className="rounded-2xl bg-blue-50 px-4 py-3">
                <div className="flex items-center justify-between gap-3">
                  <span className="text-sm font-semibold text-blue-900">Vendor</span>
                  <strong className="text-sm text-blue-900">
                    {money.format(statistics.vendorRevenue)}
                  </strong>
                </div>
                <p className="mt-1 text-xs text-blue-700">
                  {statistics.vendorPayments} {text('payment đã trả', 'paid payments')}
                </p>
              </div>
            </div>
          </div>
        </Card>
      </div>

      <Card className="mt-6">
        <div className="mb-5 flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-lg font-semibold text-slate-900">
              {text('Hoạt động theo thời gian', 'Activity over time')}
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              {text(
                'Lượt nghe, geofence, feedback và Guest Session mới trong khoảng đã chọn.',
                'Listening, geofence, feedback and new Guest sessions in the selected period.'
              )}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-4 text-xs text-slate-600">
            <span className="flex items-center gap-2">
              <span className="h-3 w-3 rounded bg-indigo-500" />
              {text('Lượt nghe', 'Listening')}
            </span>
            <span className="flex items-center gap-2">
              <span className="h-3 w-3 rounded bg-violet-500" /> Geofence
            </span>
            <span className="flex items-center gap-2">
              <span className="h-3 w-3 rounded bg-amber-500" /> Feedback
            </span>
            <span className="flex items-center gap-2">
              <span className="h-3 w-3 rounded bg-teal-600" />
              {text('Guest Session mới', 'New Guest sessions')}
            </span>
          </div>
        </div>

        {statistics.activity.length === 0 ? (
          <div className="flex h-56 items-center justify-center rounded-2xl bg-slate-50 text-sm text-slate-400">
            {text('Chưa có hoạt động trong khoảng này.', 'No activity in this period.')}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <div
              className="grid min-w-[680px] gap-3"
              style={{
                gridTemplateColumns: `repeat(${statistics.activity.length}, minmax(66px, 1fr))`
              }}
            >
              {statistics.activity.map((item) => (
                <div key={item.periodStart} className="flex flex-col items-center">
                  <div className="flex h-52 w-full items-end gap-1 rounded-2xl bg-slate-50 px-2 pt-4">
                    <div className="flex h-full min-w-0 flex-1 flex-col justify-end">
                      <span className="mb-1 text-center text-[10px] font-semibold text-indigo-700">
                        {item.listening}
                      </span>
                      <div
                        className="min-h-[2px] rounded-t bg-indigo-500"
                        style={{
                          height: `${Math.max(
                            (item.listening / maxActivity) * 100,
                            item.listening ? 4 : 1
                          )}%`
                        }}
                      />
                    </div>

                    <div className="flex h-full min-w-0 flex-1 flex-col justify-end">
                      <span className="mb-1 text-center text-[10px] font-semibold text-violet-700">
                        {item.geofence}
                      </span>
                      <div
                        className="min-h-[2px] rounded-t bg-violet-500"
                        style={{
                          height: `${Math.max(
                            (item.geofence / maxActivity) * 100,
                            item.geofence ? 4 : 1
                          )}%`
                        }}
                      />
                    </div>

                    <div className="flex h-full min-w-0 flex-1 flex-col justify-end">
                      <span className="mb-1 text-center text-[10px] font-semibold text-amber-700">
                        {item.feedbacks}
                      </span>
                      <div
                        className="min-h-[2px] rounded-t bg-amber-500"
                        style={{
                          height: `${Math.max(
                            (item.feedbacks / maxActivity) * 100,
                            item.feedbacks ? 4 : 1
                          )}%`
                        }}
                      />
                    </div>

                    <div className="flex h-full min-w-0 flex-1 flex-col justify-end">
                      <span className="mb-1 text-center text-[10px] font-semibold text-teal-700">
                        {item.guestSessions}
                      </span>
                      <div
                        className="min-h-[2px] rounded-t bg-teal-600"
                        style={{
                          height: `${Math.max(
                            (item.guestSessions / maxActivity) * 100,
                            item.guestSessions ? 4 : 1
                          )}%`
                        }}
                      />
                    </div>
                  </div>
                  <span className="mt-2 whitespace-nowrap text-xs text-slate-500">
                    {activityLabel(item.periodStart)}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
