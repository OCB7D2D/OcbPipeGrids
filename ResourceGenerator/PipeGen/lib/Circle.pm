use strict;
use warnings;
package Circle;
use constant PI => 4 * atan2(1, 1);
use base qw(Path);
use Math::Vector::Real;
use Quad;

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new;
	# Other subconstructor stuff here
	my ($radius, $details, $axis, $normal) = @_;
	# $details = 2 ** $details - 1;
	my $seg = PI / $details;
	$axis ||= V(0, 0, 1);
	$normal ||= V(0, 1, 0);
	my $vertex = $normal * $radius;
	# Add all rotated vertices to the mesh
	for (my $rot = 0; $rot < PI*2; $rot += $seg)
	{
		$self->AddVertex($axis->rotate_3d($rot, $vertex))
			->SetNormal($axis->rotate_3d($rot, $normal));
	}
	$self->AddVertex($axis->rotate_3d(0, $vertex))
		->SetNormal($axis->rotate_3d(0, $normal));
	return $self;
}

1;