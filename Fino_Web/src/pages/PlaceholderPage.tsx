import { useNavigate, useSearchParams } from 'react-router-dom';
import { Compass, Construction } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Card, CardBody } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';

export default function PlaceholderPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const title = params.get('title') ?? 'This screen';

  return (
    <>
      <PageHeader title={title} subtitle="Connected to the role menu — UI on the build roadmap" />
      <Card>
        <CardBody>
          <div className="flex flex-col items-center justify-center gap-4 py-14 text-center">
            <div className="grid size-14 place-items-center rounded-2xl bg-brand-50 text-brand-600">
              <Construction className="size-7" />
            </div>
            <div className="max-w-md">
              <p className="font-display text-lg font-semibold text-slate-800">“{title}” is on its way</p>
              <p className="mt-1 text-sm text-slate-500">
                This item is served from your role-based menu. Its screen is part of the ongoing UI build —
                the API behind it is already available in Fino_API.
              </p>
            </div>
            <Button variant="outline" onClick={() => navigate('/dashboard')}>
              <Compass className="size-4" />
              Back to dashboard
            </Button>
          </div>
        </CardBody>
      </Card>
    </>
  );
}
