use strict;
use warnings;
package Curve;
use constant PI => 4 * atan2(1, 1);
use base qw(Path);
use Math::Vector::Real;
use Quad;

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new;
	$self->{loop} = 2;
	# Other subconstructor stuff here
	my ($start, $end, $details, $pow) = @_;
	$details = 2 ** $details;
	my $seg = ($end - $start) / $details;
	#my $axis = V(0, 0, 1);
	#my $normal = V(0, 1, 0);
	#my $vertex = V(0, $radius, 0);
	# Add all rotated vertices to the mesh
	for (my $x = $start; $x <= $end; $x += $seg)
	{
		my $y = $x < 0 ? ((-$x) ** $pow) : $x ** $pow;
		my $m = $pow * $x;
		my $v = $self->AddVertex(V($x, $y, 0));
		warn "ADD VERTEX $x $y\n";
		if ($m == 0) 
		{ 
			$v->SetNormal(V(0, -1, 0));
		}
		elsif ($x < 0)
		{
			my $N = V(1, -1 / $m, 0);
			$v->SetNormal($N->versor*-1);
		}
		else
		{
			my $N = V(1, -1 / $m, 0);
			$v->SetNormal($N->versor);
		}
	}
	return $self;
}

1;