CREATE OR REPLACE FUNCTION public.fn_validate_qr_code_target()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
  v_code text;
BEGIN
  SELECT code
  INTO v_code
  FROM public.target_types
  WHERE target_type_id = NEW.target_type_id;

  IF v_code IS NULL THEN
    RAISE EXCEPTION 'Target Type is invalid.';
  END IF;

  IF v_code = 'Place' THEN
    IF NEW.place_id IS NULL OR NEW.dish_id IS NOT NULL OR NEW.narration_id IS NOT NULL THEN
      RAISE EXCEPTION 'Place QR requires place_id only.';
    END IF;

  ELSIF v_code = 'Dish' THEN
    IF NEW.dish_id IS NULL OR NEW.place_id IS NOT NULL OR NEW.narration_id IS NOT NULL THEN
      RAISE EXCEPTION 'Dish QR requires dish_id only.';
    END IF;

  ELSIF v_code = 'Narration' THEN
    IF NEW.narration_id IS NULL OR NEW.place_id IS NOT NULL OR NEW.dish_id IS NOT NULL THEN
      RAISE EXCEPTION 'Narration QR requires narration_id only.';
    END IF;

  ELSE
    RAISE EXCEPTION 'Unsupported Target Type: %', v_code;
  END IF;

  RETURN NEW;
END;
$$;
SELECT proname
FROM pg_proc
WHERE proname = 'fn_validate_qr_code_target';DROP TRIGGER IF EXISTS trg_validate_qr_code_target
ON public.qr_codes;

CREATE TRIGGER trg_validate_qr_code_target
BEFORE INSERT OR UPDATE
ON public.qr_codes
FOR EACH ROW
EXECUTE FUNCTION public.fn_validate_qr_code_target();INSERT INTO public.qr_codes (
  qr_code_value,
  target_type_id,
  place_id,
  dish_id,
  narration_id,
  is_active
)
VALUES (
  'TEST_WRONG_QR',
  1,
  NULL,
  1,
  NULL,
  TRUE
);