use strict;
use warnings;
package Line;
use base qw(Vertices);
use Math::Vector::Real;
use Math::Matrix;

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new(@_);
	# Other subconstructor stuff here
	return $self;
}

# True if $a >= $b within $tolerance
sub approx_greater
{
	my ($a, $b, $tolerance) = @_;

	$tolerance //= 1e-6;

	return ($a-$b) > -$tolerance;
}

# True if $a <= $b within $tolerance
sub approx_lesser
{
	my ($a, $b, $tolerance) = @_;

	$tolerance //= 1e-6;

	return ($b-$a) > -$tolerance;
}

# True if $a == $b within $tolerance
sub approx_equal
{
	my ($a, $b, $tolerance) = @_;

	$tolerance //= 1e-6;

	return abs($a-$b) < $tolerance;
}


# Returns shortest line: [ [x1,y1,z1], [x2,y2,z2], distance ].  
# If askew lines cross (or nearly-intersect) then xyz1 and xyz2 are undefined
# and only distance is returned.
#
# Thank you to @Fnord: https://stackoverflow.com/a/18994296/14055985
sub line_intersect
{
	# Map @_ as vectors:
	my ($a0, $a1, $b0, $b1) = @_;

	my $A = ($a1-$a0);
	my $B = ($b1-$b0);

	my $magA = abs($A);
	my $magB = abs($B);

	# If length line segment:
	if ($magA == 0 || $magB == 0)
	{
		return V(undef, undef, 0);
	}

	my $_A = $A / $magA;
	my $_B = $B / $magB;

	my $cross = $_A x $_B;
	my $denom = $cross->norm2;

	# If lines are parallel (denom=0) test if lines overlap.
	# If they don't overlap then there is a closest point solution.
	# If they do overlap, there are infinite closest positions, but there is a closest distance
	#if ($denom == 0)
	if (approx_equal($denom, 0))
	{
		my $d0 = $_A * ($b0-$a0);
		my $d1 = $_A * ($b1-$a0);

		# Is segment B before A?
		#if ($d0 <= 0 && 0 >= $d1)
		if (approx_lesser($d0, 0) && approx_greater(0, $d1))
		{
			if (abs($d0) < abs($d1))
			{
				return [$a0, $b0, abs($a0-$b0)];
			}
			else
			{
				return [$a0, $b1, abs($a0-$b1)];
			}
		}
		# Is segment B after A?
		#elsif ($d0 >= $magA && $magA <= $d1)
		elsif (approx_greater($d0, $magA) && approx_lesser($magA, $d1))
		{
			if (abs($d0) < abs($d1))
			{
				return [$a1, $b0, abs($a1-$b0)];
			}
			else
			{
				return [$a1, $b1, abs($a1-$b1)];
			}
		}
		else
		{
			# Segments overlap, return distance between parallel segments
			return [undef, V(), abs((($d0*$_A)+$a0)-$b0)];
		}

	}
	else
	{
		# Lines criss-cross: Calculate the projected closest points
		my $t = ($b0 - $a0);

		# Math::Matrix won't wirth with Math::Vector::Real
		# even though they are blessed arrays, 
		# so convert them to arrays and back to refs:
		my $detA = Math::Matrix->new([ [ @$t ], [ @$_B ], [ @$cross] ])->det;
		my $detB = Math::Matrix->new([ [ @$t ], [ @$_A ], [ @$cross] ])->det;

		my $t0 = $detA / $denom;
		my $t1 = $detB / $denom;

		my $pA = $a0 + ($_A * $t0); # Projected closest point on segment A
		my $pB = $b0 + ($_B * $t1); # Projected closest point on segment A

		if ($t0 < 0)
		{
			$pA = $a0;
		}
		elsif ($t0 > $magA)
		{
			$pA = $a1;
		}

		if ($t1 < 0)
		{
			$pB = $b0;
		}
		elsif ($t1 > $magB)
		{
			$pB = $b1;
		}

		# Clamp projection A
		if ($t0 < 0 || $t0 > $magA)
		{
			my $dot = $_B * ($pA-$b0);
			if ($dot < 0)
			{
				$dot = 0;
			}
			elsif ($dot > $magB)
			{
				$dot = $magB;
			}

			$pB = $b0 + ($_B * $dot)
		}
		
		# Clamp projection B
		if ($t1 < 0 || $t1 > $magB)
		{
			my $dot = $_A * ($pB-$a0);
			if ($dot < 0)
			{
				$dot = 0;
			}
			elsif ($dot > $magA)
			{
				$dot = $magA;
			}

			$pA = $a0 + ($_A * $dot)
		}

		return [$pA, $pB, abs($pA-$pB)];
	}
}

sub IntersectLine
{
	my ($a, $b) = @_;
	return line_intersect(
		$a->Vertice(0)->Position, $a->Vertice(1)->Position,
		$b->Vertice(0)->Position, $b->Vertice(1)->Position);

}

sub UpdateNormals
{
	die "Not available";
}

1;